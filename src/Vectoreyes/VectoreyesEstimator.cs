using System;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        public EyeCenter EstimateCenter(byte[][] image)
        {
            throw new NotImplementedException();
        }

        public Gaze EstimateGaze(byte[][] image)
        {
            throw new NotImplementedException();
        }
    }
}
