namespace Vectoreyes
{
    internal static unsafe class CV
    {
        // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv#the-gradient-algorithm
        // https://www.mathworks.com/help/images/ref/imgradient.html
        // The author of this blog post chooses to simply transpose the resultant gradient in the X direction,
        // but the gradients in either direction aren't necessarily equivalent. It's unclear if the author
        // chose to do that because it didn't matter or because they weren't aware.
        public static void CentralDifferenceGradientY(float* src, float* dst, int rows, int cols)
        {
            for (var c = 0; c < cols; c++)
            {
                var grad = src[1 + c] - src[c];
                dst[c] = grad;
            }

            for (var r = 1; r < rows - 1; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var grad = (src[(r + 1) * cols + c] - src[(r - 1) * cols + c]) / 2;
                    dst[r * cols + c] = grad;
                }
            }

            for (var c = 0; c < cols; c++)
            {
                var grad = src[(rows - 1) * cols + c] - src[(rows - 2) * cols + c];
                dst[(rows - 1) * cols + c] = grad;
            }
        }

        // https://thume.ca/projects/2012/11/04/simple-accurate-eye-center-tracking-in-opencv#the-gradient-algorithm
        // https://www.mathworks.com/help/images/ref/imgradient.html
        public static void CentralDifferenceGradientX(float* src, float* dst, int rows, int cols)
        {
            for (var r = 0; r < rows; r++)
            {
                dst[r * cols] = src[r * cols + 1] - src[r * cols];
                for (var c = 1; c < cols - 1; c++)
                {
                    var grad = (src[r * cols + c + 1] - src[r * cols + (c - 1)]) / 2;
                    dst[r * rows + c] = grad;
                }
                dst[r * cols + (cols - 1)] = src[r * cols + (cols - 1)] - src[r * cols + (cols - 2)];
            }
        }
    }
}
