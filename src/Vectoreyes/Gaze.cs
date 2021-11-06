namespace Vectoreyes
{
    public class Gaze : EyeCenter
    {
        public double GazeX { get; }

        public double GazeY { get; }

        public double GazeZ { get; }

        internal Gaze(double centerX, double centerY, double gazeX, double gazeY, double gazeZ) : base(centerX, centerY)
        {
            GazeX = gazeX;
            GazeY = gazeY;
            GazeZ = gazeZ;
        }
    }
}