using System.Drawing;

namespace Vectoreyes.EyeCenters
{
    public interface IEyeCenterEstimator
    {
        /// <summary>
        /// Estimates an eye center location in the provided image.
        /// </summary>
        /// <param name="image">The eye image.</param>
        /// <returns>The predicted eye center.</returns>
        EyeCenter EstimateCenter(Bitmap image);

        /// <summary>
        /// Estimates an eye center location in the provided image. The image
        /// provided should be a non-normalized single-channel greyscale image
        /// in float format.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <returns>The predicted eye center.</returns>
        EyeCenter EstimateCenter(float[,] image);
    }
}