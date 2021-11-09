using System;

namespace Vectoreyes
{
    internal static unsafe class GaussianBlur
    {
        // Modified from https://github.com/mdymel/superfastblur/blob/master/SuperfastBlur/GaussianBlur.cs
        // which is implemented based on http://blog.ivank.net/fastest-gaussian-blur.html.
        //
        // Fixed https://github.com/mdymel/superfastblur/issues/3 and made loops non-parallel to
        // be friendlier to library users.

        public static void Blur(float* src, float* dst, int rows, int cols, int radius)
        {
            var boxes = BoxSizes(radius, 3);
            var length = rows * cols;
            BoxBlur(src, dst, length, cols, rows, (boxes[0] - 1) / 2);
            BoxBlur(dst, src, length, cols, rows, (boxes[1] - 1) / 2);
            BoxBlur(src, dst, length, cols, rows, (boxes[2] - 1) / 2);
        }

        private static int[] BoxSizes(int sigma, int n)
        {
            var wIdeal = Math.Sqrt(12 * sigma * sigma / n + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new int[n];
            for (var i = 0; i < n; i++) sizes[i] = i < m ? wl : wu;
            return sizes;
        }

        private static void BoxBlur(float* source, float* dest, int length, int w, int h, int r)
        {
            for (var i = 0; i < length; i++) dest[i] = source[i];
            BoxBlurH(dest, source, w, h, r);
            BoxBlurT(source, dest, w, h, r);
        }

        private static void BoxBlurH(float* source, float* dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for (var i = 0; i < h; i++)
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (int)Math.Round(val * iar);
                }
            }
        }

        private static void BoxBlurT(float* source, float* dest, int w, int h, int r)
        {
            var iar = (double)1 / (r + r + 1);
            for (var i = 0; i < w; i++)
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (int)Math.Round(val * iar);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (int)Math.Round(val * iar);
                    li += w;
                    ti += w;
                }
            }
        }
    }
}