using System;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        public EyeCenter EstimateCenter(float[,] image)
        {
            throw new NotImplementedException();
        }

        public Gaze EstimateGaze(float[,] image)
        {
            throw new NotImplementedException();
        }
    }
}
