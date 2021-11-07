using System;
using System.Drawing;
using Vectoreyes.EyeCenters;
using Vectoreyes.Gazes;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        // Resizing constants
        private const int SmallImageWidth = 128;
        private const int SmallImageHeight = 128;
        private const float SmallImageWidthF = 128f;
        private const float SmallImageHeightF = 128f;

        public EyeCenter EstimateCenter(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateCenter(greyImage);
        }

        public EyeCenter EstimateCenter(float[,] image)
        {
            var rows = image.GetLength(0);
            var cols = image.GetLength(1);
            var imageCopy = Utils.CopyFloatImage(image);
            var smallImage = new float[SmallImageHeight, SmallImageWidth];
            CV.Resize(imageCopy, smallImage);

            var initialEstimate = CenterEstimator.Estimate(smallImage, 0, 0, SmallImageWidth, SmallImageHeight);

            var rowScale = rows / SmallImageHeightF;
            var colScale = cols / SmallImageWidthF;
            var scaledEstimateMinR = (int)(initialEstimate.CenterY * rowScale);
            var scaledEstimateMinC = (int)(initialEstimate.CenterX * colScale);
            var scaledEstimateMaxR = (int)Math.Ceiling((initialEstimate.CenterY + 1) * rowScale);
            var scaledEstimateMaxC = (int)Math.Ceiling((initialEstimate.CenterX + 1) * colScale);
            var scaledEstimateWidth = scaledEstimateMaxC - scaledEstimateMinC;
            var scaledEstimateHeight = scaledEstimateMaxR - scaledEstimateMinR;

            return CenterEstimator.Estimate(imageCopy, scaledEstimateMinC, scaledEstimateMinR, scaledEstimateWidth, scaledEstimateHeight);
        }

        public Gaze EstimateGaze(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateGaze(greyImage);
        }

        public Gaze EstimateGaze(float[,] image)
        {
            throw new NotImplementedException();
        }
    }
}
