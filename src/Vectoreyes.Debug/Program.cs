using System;
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
                "https://images.pexels.com/photos/10057620/pexels-photo-10057620.jpeg?cs=srgb&dl=pexels-yaroslava-borz-10057620.jpg&fm=jpg").GetAwaiter().GetResult();

            Console.WriteLine("Preparing image...");
            var srcImage = new Bitmap(Image.FromStream(imageRaw));
            using var image = new DirectBitmap(srcImage.Width, srcImage.Height);
            for (var r = 0; r < srcImage.Height; r++)
            {
                for (var c = 0; c < srcImage.Width; c++)
                {
                    image.SetPixel(c, r, srcImage.GetPixel(c, r));
                }
            }

            Console.WriteLine("Blurring...");
            var blurX = new[,] { { 1 / 4f, 1 / 2f, 1 / 4f } };
            var blurY = new[,] { { 1 / 4f }, { 1 / 2f }, { 1 / 4f } };

            // Create just one extra canvas that we reuse for performance
            using var temp = new DirectBitmap(srcImage.Width, srcImage.Height);
            for (var i = 0; i < 10; i++)
            {
                // Write from image into temp
                CV.Convolve(image, temp, blurX, 0, 1);

                // Write from temp back into image
                CV.Convolve(temp, image, blurY, 1, 0);
            }

            Console.WriteLine("Saving image...");
            image.Bitmap.Save("output.bmp");
        }
    }
}
