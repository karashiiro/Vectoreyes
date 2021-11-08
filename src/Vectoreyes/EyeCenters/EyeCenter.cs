namespace Vectoreyes.EyeCenters
{
    public class EyeCenter
    {
        public int CenterX { get; }

        public int CenterY { get; }

        internal EyeCenter(int centerX, int centerY)
        {
            CenterX = centerX;
            CenterY = centerY;
        }
    }
}