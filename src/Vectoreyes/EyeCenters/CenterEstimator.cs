using System;
using System.Diagnostics;
using System.Drawing;

namespace Vectoreyes.EyeCenters
{
    internal static class CenterEstimator
    {
        /// <summary>
        /// Estimates an eye center location from an image. The array passed in will be modified.
        /// 
        /// Implemented based on Timm, F. and Barth, E. (2011). "Accurate eye centre localisation by means of gradients",
        /// with modifications from https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv.
        /// </summary>
        /// <param name="image">The image to search.</param>
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>An eye center estimate.</returns>
        public static EyeCenter Estimate(float[] image, int rows, int cols)
        {
            // We can't meaningfully calculate anything on an image this small.
            if (rows < 4 && cols < 4)
            {
                return new EyeCenter(-1, -1);
            }
            
            // Blur step:
            var imageBlurred = new float[rows, cols];
            GaussianBlur.Blur(image, imageBlurred, rows, cols, (int)Math.Sqrt(Math.Min(rows, cols)) / 2); // Radius chosen experimentally

            // Calculate gradients, gradient magnitude mean, and gradient magnitude std
            var gradResultX = new float[rows, cols];
            var gradResultY = new float[rows, cols];
            CV.CentralDifferenceGradientX(imageBlurred, gradResultX);
            CV.CentralDifferenceGradientY(imageBlurred, gradResultY);

            var gradMags = new float[rows, cols];
            var gradResult = new float[rows, cols, 2];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    gradMags[r, c] = (float)Math.Sqrt(gradResultX[r, c] * gradResultX[r, c] + gradResultY[r, c] * gradResultY[r, c]);
                }
            }

            var gradMagMean = Utils.Mean2D(gradMags);
            var gradMagStd = Utils.Std2D(gradMags, gradMagMean);
            var gradThreshold = 0.9f * gradMagStd + gradMagMean;

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gradMag = gradMags[r, c];

                    // Ignore all gradients below a threshold
                    if (gradMag < gradThreshold)
                    {
                        continue;
                    }

                    // Scale gradients to unit length
                    gradResult[r, c, 0] = gradResultX[r, c] / gradMag;
                    gradResult[r, c, 1] = gradResultY[r, c] / gradMag;
                }
            }

            // Calculate weights
            var weights = new float[rows, cols];
            CV.Negative(imageBlurred, weights);

            // Predict eye center:
            // To save time, we only calculate the objective for every Kth column/row,
            // where K is the square root of the product of the rows and columns in the
            // image.
            //
            // This gives us a rough approximation that we can then refine by repeatedly
            // square rooting the step size and calculating scores within a predicted region.
            // This saves us a huge number of Score() calculations and allows us to
            // calculate eye centers in high-resolution images in realistic amounts of time.
            var initialStep = (int)Math.Sqrt(rows * cols);
            
            // Initial step scoring
            var centerScores = new float[rows, cols];
            for (var r = 0; r < rows; r += initialStep)
            {
                for (var c = 0; c < cols; c += initialStep)
                {
                    centerScores[r, c] = Score(weights, gradResult, rows, cols, r, c);
                }
            }

            // Search for better and better objectives within regions with high surrounding
            // objectives. We stop at a step of 2 (last step = 4), sacrificing negligible
            // accuracy for a significant speedup on larger images.
            for (var lastStep = initialStep; lastStep > 2; lastStep = (int)Math.Sqrt(lastStep))
            {
                var (localMaxR, localMaxC) = Utils.Argmax2D(centerScores);
                var localMaxVal = centerScores[localMaxR, localMaxC];
                var approxThreshold = localMaxVal * 0.999999f;
                var step = (int)Math.Sqrt(lastStep);
                for (var r = 0; r < rows; r += step)
                {
                    for (var c = 0; c < cols; c += step)
                    {
                        var scoreR = Math.Min(rows - 1, (int)(Math.Round(r / (float)lastStep) * lastStep));
                        var scoreC = Math.Min(cols - 1, (int)(Math.Round(c / (float)lastStep) * lastStep));
                        if (centerScores[scoreR, scoreC] > approxThreshold)
                        {
                            centerScores[r, c] = Score(weights, gradResult, rows, cols, r, c);
                        }
                    }
                }
            }
            
            // Calculate final estimated center
            var (maxR, maxC) = Utils.Argmax2D(centerScores);
            
            return new EyeCenter(maxC, maxR);
        }

        /// <summary>
        /// Calculate an eye center score from the provided weight image, gradient image, and predicted center
        /// location. The gradients provided should be calculated with the central difference gradient function.
        /// The weights should come from taking the negative of the smoothed original image.
        ///
        /// Implemented based on Timm, F. and Barth, E. (2011). "Accurate eye centre localisation by means of gradients",
        /// with modifications from https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv.
        /// </summary>
        private static float Score(float[,] weights, float[,,] gradient, int rows, int cols, int centerR, int centerC)
        {
            var score = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gX = gradient[r, c, 0];
                    var gY = gradient[r, c, 1];
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
                    score += dg * weights[r, c];
                }
            }

            return score / (rows * cols);
        }
    }
}