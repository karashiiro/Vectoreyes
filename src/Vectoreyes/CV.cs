namespace Vectoreyes
{
    internal static class CV
    {
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
    }
}
