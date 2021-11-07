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

        public static void Convolve(DirectBitmap src, DirectBitmap dst, in float[,] kernel, int kCenterR, int kCenterC)
        {
            var flippedKernel = FlipKernel(kernel);
            (kCenterR, kCenterC) = (kCenterC, kCenterR);

            var rows = src.Height;
            var cols = src.Width;
            var kRows = flippedKernel.GetLength(0);
            var kCols = flippedKernel.GetLength(1);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var red = 0f;
                    var green = 0f;
                    var blue = 0f;
                    for (var kR = 0; kR < kRows; kR++)
                    {
                        for (var kC = 0; kC < kCols; kC++)
                        {
                            var nextR = Utils.Clamp(r + kR - kCenterR, 0, rows - 1);
                            var nextC = Utils.Clamp(c + kC - kCenterC, 0, cols - 1);
                            var px = src.GetPixel(nextC, nextR);
                            red += px.R * flippedKernel[kR, kC];
                            green += px.G * flippedKernel[kR, kC];
                            blue += px.B * flippedKernel[kR, kC];
                        }
                    }

                    dst.SetPixel(c, r, Color.FromArgb((byte)red, (byte)green, (byte)blue));
                }
            }
        }
    }
}
