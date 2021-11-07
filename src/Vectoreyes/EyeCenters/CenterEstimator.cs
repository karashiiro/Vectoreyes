using System;
using System.Diagnostics;

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
        /// <param name="x">The x-coordinate of the objective function search space.</param>
        /// <param name="y">The y-coordinate of the objective function search space.</param>
        /// <param name="w">The width of the objective function search space.</param>
        /// <param name="h">The height of the objective function search space.</param>
        /// <returns>An eye center estimate.</returns>
        public static EyeCenter Estimate(float[,] image, int x, int y, int w, int h)
        {
            var beforeScoringLoop = new Stopwatch();
            beforeScoringLoop.Start();
            
            var rows = image.GetLength(0);
            var cols = image.GetLength(1);

            // Blur step:
            // Create just one extra matrix that we reuse for performance.
            // We need the extra matrix to avoid convolution steps affecting
            // each other.
            var temp = new float[rows, cols];
            for (var i = 0; i < 6; i++)
            {
                // Write from imageCopy into temp
                CV.Convolve(image, temp, Kernels.GaussianBlurX, 0, 1);

                // Write from temp back into imageCopy
                CV.Convolve(temp, image, Kernels.GaussianBlurY, 1, 0);
            }

            // Calculate gradients, gradient magnitude mean, and gradient magnitude std
            var gradResultX = new float[rows, cols];
            var gradResultY = new float[rows, cols];
            CV.CentralDifferenceGradientX(image, gradResultX);
            CV.CentralDifferenceGradientY(image, gradResultY);

            var gradMags = new float[rows, cols];
            var gradMagStd = 0f;
            var gradMagMean = 0f;
            var gradResult = new float[rows, cols, 2];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    gradMags[r, c] = (float)Math.Sqrt(gradResultX[r, c] * gradResultX[r, c] + gradResultY[r, c] * gradResultY[r, c]);
                    gradMagMean += gradMags[r, c];
                }
            }
            gradMagMean /= rows * cols;

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    gradMagStd += (gradMags[r, c] - gradMagMean) * (gradMags[r, c] - gradMagMean);
                }
            }
            gradMagStd = (float)Math.Sqrt(gradMagStd / (rows * cols - 1));

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gradMag = gradMags[r, c];

                    // Ignore all gradients below a threshold
                    var gradThreshold = 0.3 * gradMagStd + gradMagMean;
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
            CV.Negative(image, weights);

            // Predict eye center
            beforeScoringLoop.Stop();
            Console.WriteLine("At scoring loop, elapsed time: {0}ms", beforeScoringLoop.ElapsedMilliseconds);

            var centerScores = new float[rows, cols];
            var rowMax = y + h;
            var colMax = x + w;
            for (var r = y; r < rowMax; r++)
            {
                for (var c = x; c < colMax; c++)
                {
                    centerScores[r, c] = Score(weights, gradResult, r, c);
                }
            }

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
        private static float Score(float[,] weights, float[,,] gradient, int centerR, int centerC)
        {
            var rows = gradient.GetLength(0);
            var cols = gradient.GetLength(1);
            var score = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gX = gradient[r, c, 0];
                    var gY = gradient[r, c, 1];
                    if (gX == 0 && gY == 0)
                    {
                        continue;
                    }

                    // Calculate displacement
                    var dX = c - centerC;
                    var dY = r - centerR;
                    if (dX == 0 && dY == 0)
                    {
                        continue;
                    }

                    var dMag = Math.Sqrt(dX * dX + dY * dY);
                    var dXf = dX / dMag;
                    var dYf = dY / dMag;

                    // Dot product of displacement and gradient.
                    // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv/#the-little-thing-that-he-didnt-mention
                    // (Paraphrasing for clarity)
                    // The central difference gradient always points towards lighter regions, so
                    // the gradient vectors along the iris edge always point away from the sclera.
                    // At the center, the gradient vectors will be pointing in the same direction
                    // as the displacement vector. (continues below)
                    var dg = (float)Math.Max(0, dXf * gX + dYf * gY);

                    // In this step, we would normally square the dot product, presumably in order
                    // to penalize very small intermediate results and give a bonus to very large
                    // intermediate results. This has the unfortunate side-effect of making very
                    // negative intermediate results have an outsized impact on the objective
                    // function, and often causing estimated centers in locations where the dot
                    // products along the edge of the iris should be penalizing the estimated center.
                    //
                    // To resolve this, we choose to ignore any dot products less than zero.
                    // It may be argued that the squaring step may also be removed at this point,
                    // but I find that keeping it reduces the number of potential centers
                    // significantly, which may impact accuracy in some cases. I couldn't find
                    // any case in which this is true, but such a case may exist.
                    score += weights[r, c] * dg * dg;
                }
            }

            return score / (weights.GetLength(0) * weights.GetLength(1));
        }
    }
}