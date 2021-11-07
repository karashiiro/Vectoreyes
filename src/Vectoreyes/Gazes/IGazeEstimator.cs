using System.Drawing;

namespace Vectoreyes.Gazes
{
    public interface IGazeEstimator
    {
        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should contain both the eye center and the
        /// inner corner of the eye.
        /// </summary>
        /// <param name="image">The eye image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        Gaze EstimateGaze(Bitmap image);

        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should be a non-normalized single-channel greyscale
        /// image in float format containing both the eye center and the
        /// inner corner of the eye.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        Gaze EstimateGaze(float[,] image);
    }
}