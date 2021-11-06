namespace Vectoreyes
{
    public class EyeCenter
    {
        public float CenterX { get; }

        public float CenterY { get; }

        internal EyeCenter(float centerX, float centerY)
        {
            CenterX = centerX;
            CenterY = centerY;
        }
    }
}