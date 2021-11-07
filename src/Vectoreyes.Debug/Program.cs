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
                "https://images.pexels.com/photos/32267/pexels-photo.jpg?cs=srgb&dl=pexels-skitterphoto-32267.jpg&fm=jpg").GetAwaiter().GetResult();

            Console.WriteLine("Preparing image...");
            var fullImage = new Bitmap(Image.FromStream(imageRaw));
            var srcImage = new Bitmap(fullImage.Width / 16, fullImage.Height / 16);
            using (var g = Graphics.FromImage(srcImage))
            {
                var dstRect = new Rectangle(0, 0, srcImage.Width, srcImage.Height);
                var srcRect = new Rectangle(0, 0, fullImage.Width, fullImage.Height);
                g.DrawImage(fullImage, dstRect, srcRect, GraphicsUnit.Pixel);
            }

            srcImage.Save("output.bmp");
            
            Console.WriteLine("Target dimensions: ({0}, {1})", srcImage.Width, srcImage.Height);

            var timer = new Stopwatch();
            timer.Start();
            var eyeCenter = new VectoreyesEstimator().EstimateCenter(srcImage);
            timer.Stop();

            Console.WriteLine("Predicted center: ({0}, {1})", eyeCenter.CenterX, eyeCenter.CenterY);
            Console.WriteLine("Calculation time: {0}ms", timer.ElapsedMilliseconds);
        }
    }
}
