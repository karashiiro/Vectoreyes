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
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>The predicted eye center.</returns>
        EyeCenter EstimateCenter(float[] image, int rows, int cols);

        /// <summary>
        /// Creates an eye center estimator with prebuilt buffers for a specific
        /// image size. The estimator may not be used for images of different sizes
        /// than it was created for.
        /// </summary>
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>The reusable eye center estimator.</returns>
        EyeCenterEstimator CreateReusableEstimator(int rows, int cols);
    }
}