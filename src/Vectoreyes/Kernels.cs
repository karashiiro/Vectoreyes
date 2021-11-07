namespace Vectoreyes
{
    internal static class Kernels
    {
        public static readonly float[,] GaussianBlurX = { { 1 / 4f, 1 / 2f, 1 / 4f } };
        public static readonly float[,] GaussianBlurY = { { 1 / 4f }, { 1 / 2f }, { 1 / 4f } };
    }
}