using System;
using System.Drawing;

namespace Vectoreyes
{
    internal static class Utils
    {
        public static float Std2D(float[] x, int rows, int cols, float mean)
        {
            var std = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    std += (x[r * cols + c] - mean) * (x[r * cols + c] - mean);
                }
            }
            std = (float)Math.Sqrt(std / (rows * cols - 1));
            return std;
        }

        public static float Mean2D(float[] x, int rows, int cols)
        {
            var mean = 0f;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    mean += x[r * cols + c];
                }
            }
            mean /= rows * cols;
            return mean;
        }

        /// <summary>
        /// Calculate the position of the highest value in the array.
        /// </summary>
        /// <param name="x">The input 2D array.</param>
        /// <returns>The position, in (row, col) order.</returns>
        public static (int, int) Argmax2D(float[] x, int rows, int cols)
        {
            var maxVal = 0f;
            var maxR = -1;
            var maxC = -1;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (x[r * cols + c] > maxVal)
                    {
                        maxVal = x[r * cols + c];
                        maxR = r;
                        maxC = c;
                    }
                }
            }

            return (maxR, maxC);
        }

        public static float[] Bitmap2GreyArray(Bitmap srcImage)
        {
            var image = new float[srcImage.Height * srcImage.Width];
            for (var r = 0; r < srcImage.Height; r++)
            {
                for (var c = 0; c < srcImage.Width; c++)
                {
                    var px = srcImage.GetPixel(c, r);

                    // https://en.wikipedia.org/wiki/Grayscale#Colorimetric_(perceptual_luminance-preserving)_conversion_to_grayscale
                    image[r * srcImage.Width + c] = px.R * 0.2126f + px.G * 0.7152f + px.B * 0.0722f;
                }
            }

            return image;
        }

        public static void SaveScoresImage(float[] centerScores, int rows, int cols, string filename)
        {
            var (maxR, maxC) = Argmax2D(centerScores, rows, cols);
            var maxVal = centerScores[maxR * cols + maxC];
            
            var bmp = new Bitmap(cols, rows);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var px = (int)(255 * centerScores[r * cols + c] / maxVal);
                    bmp.SetPixel(c, r, Color.FromArgb(px, 255 - px, 0));
                }
            }
            bmp.Save(filename);
        }

        public static void SaveGreyImage(float[] image, int rows, int cols, string filename)
        {
            var bmp = new Bitmap(cols, rows);
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var px = (int)image[r * cols + c];
                    bmp.SetPixel(c, r, Color.FromArgb(px, px, px));
                }
            }
            bmp.Save(filename);
        }

        public static float[] CopyFloatImage(float[] src)
        {
            var dst = new float[src.Length];
            for (var i = 0; i < src.Length; i++)
            {
                dst[i] = src[i];
            }

            return dst;
        }

        public static int Clamp(int x, int min, int max)
        {
            return Math.Max(Math.Min(max, x), min);
        }
    }
}