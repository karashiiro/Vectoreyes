using System;
using System.Drawing;

namespace Vectoreyes
{
    internal static class Utils
    {
        public static float Std2D(float[,] x, float mean)
        {
            var rows = x.GetLength(0);
            var cols = x.GetLength(1);
            var std = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    std += (x[r, c] - mean) * (x[r, c] - mean);
                }
            }
            std = (float)Math.Sqrt(std / (rows * cols - 1));
            return std;
        }

        public static float Mean2D(float[,] x)
        {
            var rows = x.GetLength(0);
            var cols = x.GetLength(1);
            var mean = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    mean += x[r, c];
                }
            }
            mean /= rows * cols;
            return mean;
        }

        public static (int, int) Argmax2D(float[,] x)
        {
            var rows = x.GetLength(0);
            var cols = x.GetLength(1);
            var maxVal = 0f;
            var maxR = -1;
            var maxC = -1;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (x[r, c] > maxVal)
                    {
                        maxVal = x[r, c];
                        maxR = r;
                        maxC = c;
                    }
                }
            }

            return (maxR, maxC);
        }

        public static float[,] Bitmap2GreyArray(Bitmap srcImage)
        {
            var image = new float[srcImage.Height, srcImage.Width];
            for (var r = 0; r < srcImage.Height; r++)
            {
                for (var c = 0; c < srcImage.Width; c++)
                {
                    var px = srcImage.GetPixel(c, r);

                    // https://en.wikipedia.org/wiki/Grayscale#Colorimetric_(perceptual_luminance-preserving)_conversion_to_grayscale
                    image[r, c] = px.R * 0.2126f + px.G * 0.7152f + px.B * 0.0722f;
                }
            }

            return image;
        }

        public static float[,] CopyFloatImage(float[,] src)
        {
            var rows = src.GetLength(0);
            var cols = src.GetLength(1);
            var dst = new float[rows, cols];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    dst[r, c] = src[r, c];
                }
            }

            return dst;
        }

        public static int Clamp(int x, int min, int max)
        {
            return Math.Max(Math.Min(max, x), min);
        }
    }
}