namespace Vectoreyes
{
    public interface IEyeCenterEstimator
    {
        /// <summary>
        /// Estimates an eye center location in the provided image. The image
        /// provided should be a single-channel greyscale image.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <returns>The predicted eye center.</returns>
        EyeCenter EstimateCenter(byte[][] image);
    }
}