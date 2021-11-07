using System;
using System.Drawing;
using Vectoreyes.EyeCenters;
using Vectoreyes.Gazes;

namespace Vectoreyes
{
    public class VectoreyesEstimator : IEyeCenterEstimator, IGazeEstimator
    {
        private static readonly float[,] GaussianBlurX = { { 1 / 4f, 1 / 2f, 1 / 4f } };
        private static readonly float[,] GaussianBlurY = { { 1 / 4f }, { 1 / 2f }, { 1 / 4f } };

        public EyeCenter EstimateCenter(Bitmap image)
        {
            var greyImage = Utils.Bitmap2GreyArray(image);
            return EstimateCenter(greyImage);
        }

        public EyeCenter EstimateCenter(float[,] image)
        {
            var imageCopy = Utils.CopyFloatImage(image);

            var rows = imageCopy.GetLength(0);
            var cols = imageCopy.GetLength(1);

            // Blur step:
            // Create just one extra matrix that we reuse for performance.
            // We need the extra matrix to avoid convolution steps affecting
            // each other.
            var temp = new float[rows, cols];
            for (var i = 0; i < 6; i++)
            {
                // Write from imageCopy into temp
                CV.Convolve(imageCopy, temp, GaussianBlurX, 0, 1);

                // Write from temp back into imageCopy
                CV.Convolve(temp, imageCopy, GaussianBlurY, 1, 0);
            }

            // Calculate gradients, gradient magnitude mean, and gradient magnitude std
            var gradResultX = new float[rows, cols];
            var gradResultY = new float[rows, cols];
            CV.CentralDifferenceGradientX(imageCopy, gradResultX);
            CV.CentralDifferenceGradientY(imageCopy, gradResultY);

            var gradMags = new float[rows, cols];
            var gradMagStd = 0f;
            var gradMagMean = 0f;
            var gradResult = new float[rows, cols, 2];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    gradMags[r, c] = (float)Math.Sqrt(Math.Pow(gradResultX[r, c], 2) + Math.Pow(gradResultY[r, c], 2));
                    gradMagMean += gradMags[r, c];
                }
            }
            gradMagMean /= rows * cols;

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    gradMagStd += (float)Math.Pow(gradMags[r, c] - gradMagMean, 2);
                }
            }
            gradMagStd = (float)Math.Sqrt(gradMagStd / (rows * cols - 1));

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var gradMag = gradMags[r, c];

                    // Ignore all gradients below a threshold
                    var gradThreshold = 0.3 * gradMagStd + gradMagMean;
                    if (gradMag < gradThreshold)
                    {
                        continue;
                    }

                    // Scale gradients to unit length
                    gradResult[r, c, 0] = gradResultX[r, c] / gradMag;
                    gradResult[r, c, 1] = gradResultY[r, c] / gradMag;
                }
            }

            // Calculate weights
            var weights = new float[rows, cols];
            CV.Negative(imageCopy, weights);

            // Predict eye center
            var maxScorePre = 0f;
            var centerScores = new float[rows, cols];
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    centerScores[r, c] = CV.EyeCenterScore(weights, gradResult, r, c);
                    if (centerScores[r, c] > maxScorePre)
                    {
                        maxScorePre = centerScores[r, c];
                    }
                }
            }
            
            var maxScore = 0f;
            var maxR = -1;
            var maxC = -1;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (centerScores[r, c] > maxScore)
                    {
                        maxScore = centerScores[r, c];
                        maxR = r;
                        maxC = c;
                    }
                }
            }

            return new EyeCenter(maxC, maxR);
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
