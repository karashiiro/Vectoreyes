using System;
using System.Drawing;

namespace Vectoreyes
{
    internal static class CV
    {
        private static float[,] FlipKernel(in float[,] kernel)
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

        public static void Convolve(in float[,] src, in float[,] dst, in float[,] kernel, int kCenterR, int kCenterC)
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
    }
}
