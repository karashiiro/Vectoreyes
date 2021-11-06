namespace Vectoreyes
{
    public class EyeCenter
    {
        public double CenterX { get; }

        public double CenterY { get; }

        internal EyeCenter(double centerX, double centerY)
        {
            CenterX = centerX;
            CenterY = centerY;
        }
    }
}