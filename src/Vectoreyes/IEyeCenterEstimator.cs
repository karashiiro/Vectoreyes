namespace Vectoreyes
{
    public interface IEyeCenterEstimator
    {
        /// <summary>
        /// Estimates an eye center location in the provided image. The image
        /// provided should be a single-channel greyscale image.
        /// </summary>
        /// <param name="image">The greyscale image.</param>
        /// <returns>The predicted eye center.</returns>
        EyeCenter EstimateCenter(byte[][] image);

        /// <summary>
        /// Estimates an eye center location in the provided image. The image
        /// provided should be a single-channel greyscale image.
        /// </summary>
        /// <param name="image">The greyscale image.</param>
        /// <param name="r">The number of rows in the image.</param>
        /// <param name="c">The number of columns in the image.</param>
        /// <returns>The predicted eye center.</returns>
        unsafe EyeCenter EstimateCenter(byte* image, int r, int c);
    }
}