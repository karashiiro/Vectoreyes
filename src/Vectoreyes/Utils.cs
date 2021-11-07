using System;

namespace Vectoreyes
{
    public class Utils
    {
        public static int Clamp(int x, int min, int max)
        {
            return Math.Max(Math.Min(max, x), min);
        }
    }
}