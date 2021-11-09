using System;
using System.Drawing;
using Vectoreyes.EyeCenters;
using Vectoreyes.Gazes;

namespace Vectoreyes
{
    public class VectoreyesEstimator
    {
        /// <summary>
        /// Estimates an eye center location in the provided image.
        /// </summary>
        /// <param name="image">The eye image.</param>
        /// <returns>The predicted eye center.</returns>
        public static EyeCenter EstimateCenter(Bitmap image)
        {
            var greyImage = new float[image.Height * image.Width];
            Utils.Bitmap2GreyArray(image, greyImage);
            return EstimateCenter(greyImage, image.Height, image.Width);
        }

        /// <summary>
        /// Estimates an eye center location in the provided image. The image
        /// provided should be a non-normalized single-channel greyscale image
        /// in float format.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>The predicted eye center.</returns>
        public static EyeCenter EstimateCenter(float[] image, int rows, int cols)
        {
            var imageCopy = Utils.CopyFloatImage(image);
            return new EyeCenterEstimator(rows, cols).Estimate(imageCopy);
        }

        /// <summary>
        /// Creates an eye center estimator with prebuilt buffers for a specific
        /// image size. The estimator may not be used for images of different sizes
        /// than it was created for.
        /// </summary>
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>The reusable eye center estimator.</returns>
        public static EyeCenterEstimator CreateReusableEstimator(int rows, int cols)
        {
            return new EyeCenterEstimator(rows, cols);
        }

        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should contain both the eye center and the
        /// inner corner of the eye.
        /// </summary>
        /// <param name="image">The eye image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        public static Gaze EstimateGaze(Bitmap image)
        {
            var greyImage = new float[image.Height * image.Width];
            Utils.Bitmap2GreyArray(image, greyImage);
            return EstimateGaze(greyImage, image.Height, image.Width);
        }

        /// <summary>
        /// Estimates a gaze vector from the provided image. The image
        /// provided should be a non-normalized single-channel greyscale
        /// image in float format containing both the eye center and the
        /// inner corner of the eye.
        /// </summary>
        /// <param name="image">The greyscale eye image.</param>
        /// <param name="rows">The number of rows in the image.</param>
        /// <param name="cols">The number of columns in the image.</param>
        /// <returns>The predicted gaze and associated eye center.</returns>
        public static Gaze EstimateGaze(float[] image, int rows, int cols)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a source bitmap into a floating-point greyscale array.
        /// </summary>
        /// <param name="src">The source bitmap.</param>
        /// <param name="dst">The destination array.</param>
        public void BitmapToFloatGrey(Bitmap src, float[] dst)
        {
            Utils.Bitmap2GreyArray(src, dst);
        }

        /// <summary>
        /// Converts a byte greyscale array into a floating-point greyscale array.
        /// </summary>
        /// <param name="src">The source array.</param>
        /// <param name="dst">The destination array.</param>
        public void ByteGreyToFloatGrey(byte[] src, float[] dst)
        {
            for (var i = 0; i < src.Length; i++)
            {
                dst[i] = (float)src[i] / 255;
            }
        }
    }
}
