namespace Vectoreyes
{
    public interface IGazeEstimator
    {
        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should be a single-channel greyscale image containing
        /// both the eye center and the inner corner of the eye.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        Gaze EstimateGaze(byte[][] image);
    }
}