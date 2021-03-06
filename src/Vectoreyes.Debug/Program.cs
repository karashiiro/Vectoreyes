using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using Vectoreyes.EyeCenters;

namespace Vectoreyes.Debug
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Downloading image...");
            using var http = new HttpClient();
            var imageRaw = http.GetStreamAsync(
                "https://raw.githubusercontent.com/karashiiro/justeyecenters/main/example/eyes/0.jpg").GetAwaiter().GetResult();
            
            var srcImage = new Bitmap(Image.FromStream(imageRaw));
            var buf = new float[srcImage.Height * srcImage.Width];
            Utils.Bitmap2GreyArray(srcImage, buf);
            Console.WriteLine("Target dimensions: ({0}, {1})", srcImage.Width, srcImage.Height);

            var estimator = VectoreyesEstimator.CreateReusableEstimator(srcImage.Height, srcImage.Width);

            var timer1 = new Stopwatch();
            timer1.Start();
            estimator.Estimate(buf);
            timer1.Stop();
            Console.WriteLine("Calculation time: {0}ms", timer1.ElapsedMilliseconds);

            var eyeCenter = new EyeCenter(-1, -1);
            var timer2 = new Stopwatch();
            timer2.Start();
            for (var i = 0; i < 1000; i++)
            {
                eyeCenter = estimator.Estimate(buf);
            }
            timer2.Stop();

            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    srcImage.SetPixel(eyeCenter.CenterX + j, eyeCenter.CenterY + i, Color.Red);
                }
            }
            srcImage.Save("output.bmp");

            Console.WriteLine("Predicted center: ({0}, {1})", eyeCenter.CenterX, eyeCenter.CenterY);
            Console.WriteLine("Calculation time (1000x): {0}ms", timer2.ElapsedMilliseconds);
        }
    }
}
