using System;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        public EyeCenter EstimateCenter(byte[][] image)
        {
            throw new NotImplementedException();
        }

        public unsafe EyeCenter EstimateCenter(byte* image, int r, int c)
        {
            throw new NotImplementedException();
        }

        public Gaze EstimateGaze(byte[][] image)
        {
            throw new NotImplementedException();
        }

        public unsafe Gaze EstimateGaze(byte* image, int r, int c)
        {
            throw new NotImplementedException();
        }
    }
}
