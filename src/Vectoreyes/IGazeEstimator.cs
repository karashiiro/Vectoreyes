namespace Vectoreyes
{
    public interface IGazeEstimator
    {
        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should be a single-channel greyscale image.
        /// </summary>
        /// <param name="image">The greyscale image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        Gaze EstimateGaze(byte[][] image);

        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should be a single-channel greyscale image.
        /// </summary>
        /// <param name="image">The greyscale image.</param>
        /// <param name="r">The number of rows in the image.</param>
        /// <param name="c">The number of columns in the image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        unsafe Gaze EstimateGaze(byte* image, int r, int c);
    }
}