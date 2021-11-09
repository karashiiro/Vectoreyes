using System;
using System.Runtime.InteropServices;

namespace Vectoreyes.EyeCenters
{
    /// <summary>
    /// Eye center estimation class with pre-built buffers for reuse. This class is not thread-safe.
    /// </summary>
    public unsafe class EyeCenterEstimator : IDisposable
    {
        private GCHandle _hImageBlurred;
        private GCHandle _hGradResultX;
        private GCHandle _hGradResultY;
        private GCHandle _hGradResult;
        private GCHandle _hGradMags;
        private GCHandle _hCenterScores;

        private readonly float* _imageBlurred;
        private readonly float* _gradResultX;
        private readonly float* _gradResultY;
        private readonly float* _gradResult;
        private readonly float* _gradMags;
        private readonly float* _centerScores;

        private readonly int _rows;
        private readonly int _cols;
        private readonly int _length;

        internal EyeCenterEstimator(int rows, int cols)
        {
            var indices = rows * cols;

            _hImageBlurred = GCHandle.Alloc(new float[indices], GCHandleType.Pinned);
            _hGradResultX = GCHandle.Alloc(new float[indices], GCHandleType.Pinned);
            _hGradResultY = GCHandle.Alloc(new float[indices], GCHandleType.Pinned);
            _hGradResult = GCHandle.Alloc(new float[indices * 2], GCHandleType.Pinned);
            _hGradMags = GCHandle.Alloc(new float[indices], GCHandleType.Pinned);
            _hCenterScores = GCHandle.Alloc(new float[indices], GCHandleType.Pinned);

            _imageBlurred = (float*)_hImageBlurred.AddrOfPinnedObject();
            _gradResultX = (float*)_hGradResultX.AddrOfPinnedObject();
            _gradResultY = (float*)_hGradResultY.AddrOfPinnedObject();
            _gradResult = (float*)_hGradResult.AddrOfPinnedObject();
            _gradMags = (float*)_hGradMags.AddrOfPinnedObject();
            _centerScores = (float*)_hCenterScores.AddrOfPinnedObject();

            _rows = rows;
            _cols = cols;
            _length = rows * cols;
        }

        /// <summary>
        /// Estimates an eye center location from an image. The array passed in will be modified.
        /// 
        /// Implemented based on Timm, F. and Barth, E. (2011). "Accurate eye centre localisation by means of gradients",
        /// with modifications from https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv.
        /// </summary>
        /// <param name="image">The image to search.</param>
        /// <returns>An eye center estimate.</returns>
        public EyeCenter Estimate(float[] image)
        {
            fixed (float* im = &image[0])
            {
                // We can't meaningfully calculate anything on an image this small.
                if (_rows < 4 && _cols < 4)
                {
                    return new EyeCenter(-1, -1);
                }

                // Zero scores
                for (var i = 0; i < _length; i++)
                {
                    _centerScores[i] = 0;
                }

                // Blur step:
                GaussianBlur.Blur(im, _imageBlurred, _rows, _cols, (int)Math.Sqrt(Math.Min(_rows, _cols)) / 2); // Radius chosen experimentally

                // Calculate gradients, gradient magnitude mean, and gradient magnitude std
                CV.CentralDifferenceGradientX(_imageBlurred, _gradResultX, _rows, _cols);
                CV.CentralDifferenceGradientY(_imageBlurred, _gradResultY, _rows, _cols);

                for (var r = 0; r < _rows; r++)
                {
                    for (var c = 0; c < _cols; c++)
                    {
                        _gradMags[r * _cols + c] = (float)Math.Sqrt(_gradResultX[r * _cols + c] * _gradResultX[r * _cols + c] + _gradResultY[r * _cols + c] * _gradResultY[r * _cols + c]);
                    }
                }

                var gradMagMean = Utils.Mean2D(_gradMags, _rows, _cols);
                var gradMagStd = Utils.Std2D(_gradMags, _rows, _cols, gradMagMean);
                var gradThreshold = 0.9f * gradMagStd + gradMagMean;

                for (var r = 0; r < _rows; r++)
                {
                    for (var c = 0; c < _cols; c++)
                    {
                        var gradMag = _gradMags[r * _cols + c];

                        // Ignore all gradients below a threshold
                        if (gradMag < gradThreshold)
                        {
                            continue;
                        }

                        // Scale gradients to unit length
                        _gradResult[r * _cols + c + 1] = _gradResultY[r * _cols + c] / gradMag;
                        _gradResult[r * _cols + c] = _gradResultX[r * _cols + c] / gradMag;
                    }
                }

                // Predict eye center:
                // To save time, we only calculate the objective for every Kth column/row,
                // where K is the square root of the product of the rows and columns in the
                // image.
                //
                // This gives us a rough approximation that we can then refine by repeatedly
                // square rooting the step size and calculating scores within a predicted region.
                // This saves us a huge number of Score() calculations and allows us to
                // calculate eye centers in high-resolution images in realistic amounts of time.
                var initialStep = (int)Math.Sqrt(_rows * _cols);

                // Initial step scoring
                for (var r = 0; r < _rows; r += initialStep)
                {
                    for (var c = 0; c < _cols; c += initialStep)
                    {
                        _centerScores[r * _cols + c] = Score(_gradResult, _rows, _cols, r, c);
                    }
                }

                // Search for better and better objectives within regions with high surrounding
                // objectives. We stop at a step of 2 (last step = 4), sacrificing negligible
                // accuracy for a significant speedup on larger images.
                for (var lastStep = initialStep; lastStep > 2; lastStep = (int)Math.Sqrt(lastStep))
                {
                    var (localMaxR, localMaxC) = Utils.Argmax2D(_centerScores, _rows, _cols);
                    var localMaxVal = _centerScores[localMaxR * _cols + localMaxC];
                    var approxThreshold = localMaxVal * 0.999999f;
                    var step = (int)Math.Sqrt(lastStep);
                    for (var r = 0; r < _rows; r += step)
                    {
                        for (var c = 0; c < _cols; c += step)
                        {
                            var scoreR = Math.Min(_rows - 1, (int)(Math.Round(r / (float)lastStep) * lastStep));
                            var scoreC = Math.Min(_cols - 1, (int)(Math.Round(c / (float)lastStep) * lastStep));
                            if (_centerScores[scoreR * _cols + scoreC] > approxThreshold)
                            {
                                _centerScores[r * _cols + c] = Score(_gradResult, _rows, _cols, r, c);
                            }
                        }
                    }
                }

                // Calculate final estimated center
                var (maxR, maxC) = Utils.Argmax2D(_centerScores, _rows, _cols);

                return new EyeCenter(maxC, maxR);
            }
        }

        /// <summary>
        /// Calculate an eye center score from the provided weight image, gradient image, and predicted center
        /// location. The gradients provided should be calculated with the central difference gradient function.
        /// The weights should come from taking the negative of the smoothed original image.
        ///
        /// Implemented based on Timm, F. and Barth, E. (2011). "Accurate eye centre localisation by means of gradients",
        /// with modifications from https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv.
        /// </summary>
        private static float Score(float* gradient, int rows, int cols, int centerR, int centerC)
        {
            var score = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gY = gradient[r * cols + c + 1];
                    var gX = gradient[r * cols + c];
                    if (gX + gY == 0)
                    {
                        continue;
                    }

                    // Calculate displacement
                    var dX = c - centerC;
                    var dY = r - centerR;
                    if (dX + dY == 0)
                    {
                        continue;
                    }

                    var dMag = (float)Math.Sqrt(dX * dX + dY * dY);
                    var dXf = dX / dMag;
                    var dYf = dY / dMag;

                    // Dot product of displacement and gradient.
                    // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv/#the-little-thing-that-he-didnt-mention
                    // (Paraphrasing for clarity)
                    // The central difference gradient always points towards lighter regions, so
                    // the gradient vectors along the iris edge always point away from the sclera.
                    // At the center, the gradient vectors will be pointing in the same direction
                    // as the displacement vector. (continues below)
                    var dg = Math.Max(0, dXf * gX + dYf * gY);

                    // In this step, we would normally just square the dot product, presumably in order
                    // to penalize very small intermediate results and give a bonus to very large
                    // intermediate results. This has the unfortunate side-effect of making very
                    // negative intermediate results have an outsized impact on the objective
                    // function, and often causing estimated centers in locations where the dot
                    // products along the edge of the iris should be penalizing the estimated center.
                    //
                    // To resolve this, we choose to ignore any dot products less than zero.
                    // The squaring step may also be removed at this point, since our dot products
                    // are all greater than or equal to 0. In fact, doing so appears to improve
                    // accuracy.
                    //
                    // I'm also choosing to remove the pixel intensity weight here, since it reduces
                    // accuracy when there are reflections on the iris.
                    score += dg;
                }
            }

            return score / (rows * cols);
        }

        public void Dispose()
        {
            _hImageBlurred.Free();
            _hGradResultX.Free();
            _hGradResultY.Free();
            _hGradResult.Free();
            _hGradMags.Free();
            _hCenterScores.Free();
        }
    }
}