using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;

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
            Console.WriteLine("Target dimensions: ({0}, {1})", srcImage.Width, srcImage.Height);

            var timer = new Stopwatch();
            timer.Start();
            var eyeCenter = new VectoreyesEstimator().EstimateCenter(srcImage);
            timer.Stop();

            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    srcImage.SetPixel(eyeCenter.CenterX + j, eyeCenter.CenterY + i, Color.Red);
                }
            }
            srcImage.Save("output.bmp");

            Console.WriteLine("Predicted center: ({0}, {1})", eyeCenter.CenterX, eyeCenter.CenterY);
            Console.WriteLine("Calculation time: {0}ms", timer.ElapsedMilliseconds);
        }
    }
}
