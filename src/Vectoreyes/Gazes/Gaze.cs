using Vectoreyes.EyeCenters;

namespace Vectoreyes.Gazes
{
    public class Gaze : EyeCenter
    {
        public float GazeX { get; }

        public float GazeY { get; }

        public float GazeZ { get; }

        internal Gaze(int centerX, int centerY, float gazeX, float gazeY, float gazeZ) : base(centerX, centerY)
        {
            GazeX = gazeX;
            GazeY = gazeY;
            GazeZ = gazeZ;
        }
    }
}