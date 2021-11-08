using System;
using System.Drawing;
using Vectoreyes.EyeCenters;
using Vectoreyes.Gazes;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        public EyeCenter EstimateCenter(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateCenter(greyImage, image.Height, image.Width);
        }

        public EyeCenter EstimateCenter(float[] image, int rows, int cols)
        {
            var imageCopy = Utils.CopyFloatImage(image);
            return new EyeCenterEstimator(rows, cols).Estimate(imageCopy);
        }

        public EyeCenterEstimator CreateReusableEstimator(int rows, int cols)
        {
            return new EyeCenterEstimator(rows, cols);
        }

        public Gaze EstimateGaze(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateGaze(greyImage, image.Height, image.Width);
        }

        public Gaze EstimateGaze(float[] image, int rows, int cols)
        {
            throw new NotImplementedException();
        }
    }
}
