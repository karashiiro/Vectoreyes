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

        public static void Negative(float[,] src, float[,] dst)
        {
            var rows = src.GetLength(0);
            var cols = src.GetLength(1);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    dst[r, c] = 255 - src[r, c];
                }
            }
        }

        /// <summary>
        /// Resizes the source image into the destination image.
        /// </summary>
        /// <param name="src">The source image.</param>
        /// <param name="dst">The destination image.</param>
        public static void Resize(float[,] src, float[,] dst)
        {
            var srcRows = src.GetLength(0);
            var srcCols = src.GetLength(1);
            var dstRows = dst.GetLength(0);
            var dstCols = dst.GetLength(1);

            var rowScale = (float)(srcRows - 1) / (dstRows - 1);
            var colScale = (float)(srcCols - 1) / (dstCols - 1);

            for (var r = 0; r < dstRows; r++)
            {
                for (var c = 0; c < dstCols; c++)
                {
                    var srcR = (int)Math.Floor(rowScale * r);
                    var srcC = (int)Math.Floor(colScale * c);
                    dst[r, c] = BilinearInterpolation(src, srcR, srcC, rowScale * r, colScale * c);
                }
            }
        }

        public static float BilinearInterpolation(float[,] image, int r, int c, float subPixelR, float subPixelC)
        {
            var rows = image.GetLength(0);
            var cols = image.GetLength(1);

            var alpha = subPixelR - r;
            var beta = subPixelC - c;

            var px1 = (1 - alpha) * (1 - beta) * image[r, c];
            var px2 = alpha * (1 - beta) * image[Utils.Clamp(r + 1, 0, rows - 1), c];
            var px3 = (1 - alpha) * beta * image[r, Utils.Clamp(c + 1, 0, cols - 1)];
            var px4 = alpha * beta * image[Utils.Clamp(r + 1, 0, rows - 1), Utils.Clamp(c + 1, 0, cols - 1)];

            return px1 + px2 + px3 + px4;
        }
    }
}
