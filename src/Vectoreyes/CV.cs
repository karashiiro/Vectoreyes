using System;

namespace Vectoreyes
{
    internal static class CV
    {
        private static float[,] FlipKernel(float[,] kernel)
        {
            var rows = kernel.GetLength(0);
            var cols = kernel.GetLength(1);

            var output = new float[cols, rows];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    output[cols - 1 - c, rows - 1 - r] = kernel[r, c];
                }
            }

            return output;
        }

        public static void Convolve(float[,] src, float[,] dst, float[,] kernel, int kCenterR, int kCenterC)
        {
            var flippedKernel = FlipKernel(kernel);
            (kCenterR, kCenterC) = (kCenterC, kCenterR);

            var rows = src.GetLength(0);
            var cols = src.GetLength(1);
            var kRows = flippedKernel.GetLength(0);
            var kCols = flippedKernel.GetLength(1);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var val = 0f;
                    for (var kR = 0; kR < kRows; kR++)
                    {
                        for (var kC = 0; kC < kCols; kC++)
                        {
                            var nextR = Utils.Clamp(r + kR - kCenterR, 0, rows - 1);
                            var nextC = Utils.Clamp(c + kC - kCenterC, 0, cols - 1);
                            var px = src[nextR, nextC];
                            val += px * flippedKernel[kR, kC];
                        }
                    }

                    dst[r, c] = val;
                }
            }
        }

        // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv#the-gradient-algorithm
        // https://www.mathworks.com/help/images/ref/imgradient.html
        // The author of this blog post chooses to simply transpose the resultant gradient in the X direction,
        // but the gradients in either direction aren't necessarily equivalent. It's unclear if the author
        // chose to do that because it didn't matter or because they weren't aware.
        public static void CentralDifferenceGradientY(float[,] src, float[,] dst)
        {
            var rows = src.GetLength(0);
            var cols = src.GetLength(1);

            for (var c = 0; c < cols; c++)
            {
                var grad = src[1, c] - src[0, c];
                dst[0, c] = grad;
            }

            for (var r = 1; r < rows - 1; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var grad = (src[r + 1, c] - src[r - 1, c]) / 2;
                    dst[r, c] = grad;
                }
            }

            for (var c = 0; c < cols; c++)
            {
                var grad = src[rows - 1, c] - src[rows - 2, c];
                dst[rows - 1, c] = grad;
            }
        }

        // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv#the-gradient-algorithm
        // https://www.mathworks.com/help/images/ref/imgradient.html
        public static void CentralDifferenceGradientX(float[,] src, float[,] dst)
        {
            var rows = src.GetLength(0);
            var cols = src.GetLength(1);
            for (var r = 0; r < rows; r++)
            {
                dst[r, 0] = src[r, 1] - src[r, 0];
                for (var c = 1; c < cols - 1; c++)
                {
                    var grad = (src[r, c + 1] - src[r, c - 1]) / 2;
                    dst[r, c] = grad;
                }
                dst[r, cols - 1] = src[r, cols - 1] - src[r, cols - 2];
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
        public static float EyeCenterScore(float[,] weights, float[,,] gradient, int centerR, int centerC)
        {
            var score = 0f;
            for (var r = 0; r < gradient.GetLength(0); r++)
            {
                for (var c = 0; c < gradient.GetLength(1); c++)
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

                    var dMag = (float)Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
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
                    score += weights[r, c] * (float)Math.Pow(dg, 2);
                }
            }

            return score / (weights.GetLength(0) * weights.GetLength(1));
        }
    }
}
