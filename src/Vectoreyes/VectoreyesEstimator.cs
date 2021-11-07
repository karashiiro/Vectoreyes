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
            return EstimateCenter(greyImage);
        }

        public EyeCenter EstimateCenter(float[,] image)
        {
            var imageCopy = Utils.CopyFloatImage(image);
            return CenterEstimator.Estimate(imageCopy);
        }

        public Gaze EstimateGaze(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateGaze(greyImage);
        }

        public Gaze EstimateGaze(float[,] image)
        {
            throw new NotImplementedException();
        }
    }
}
