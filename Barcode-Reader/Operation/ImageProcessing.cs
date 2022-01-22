using OpenCvSharp;
using System;
using System.Linq;

namespace Barcode_Reader
{
    public static class ImageProcessing
    {
        public static Mat GetBarcodeRegion(Mat mat, out Rect region, bool debug)
        {
            // Assign region variable
            region = new Rect();

            // Create output Mat
            Mat result = new Mat();


            using (Mat gray = new Mat())
            using (Mat gradX = new Mat())
            using (Mat gradY = new Mat())
            using (Mat gradient = new Mat())
            using (Mat blurred = new Mat())
            using (Mat thresholded = new Mat())
            using (Mat kernel = Cv2.GetStructuringElement(shape: MorphShapes.Rect, ksize: new Size(width: 21, height: 7)))
            using (Mat closed = new Mat())
            {
                // 1. Convert it to grayscale
                if (mat.Channels() > 1)
                    Cv2.CvtColor(src: mat, dst: gray, ColorConversionCodes.BGR2GRAY);
                else
                    mat.CopyTo(gray);

                if (debug)
                {
                    //gray.CopyTo(result);
                }

                // 2. Compute the Scharr gradient magnitude representation of the image in both X and Y direction
                Cv2.Sobel(src: gray, dst: gradX, ddepth: MatType.CV_32F, xorder: 1, yorder: 0, ksize: -1);
                Cv2.Sobel(src: gray, dst: gradY, ddepth: MatType.CV_32F, xorder: 0, yorder: 1, ksize: -1);

                // subtract the y-gradient from the x-gradient
                Cv2.Subtract(src1: gradX, src2: gradY, dst: gradient);
                Cv2.ConvertScaleAbs(src: gradient, dst: gradient);

                if (debug)
                {
                    //gradient.CopyTo(result);
                }

                // 3. Blur and threshold the image
                Cv2.Blur(src: gradient, dst: blurred, ksize: new Size(9, 9));
                Cv2.Threshold(src: blurred, dst: thresholded, thresh: 190, maxval: 255, type: ThresholdTypes.Binary);

                if (debug)
                {
                    //threshold.CopyTo(result);
                }

                // 4. Construct a closing kernel and apply it to the thresholded image
                Cv2.MorphologyEx(src: thresholded, dst: closed, op: MorphTypes.Close, element: kernel);

                // perform a series of erosions and dilations
                Cv2.Erode(src: closed, dst: closed, element: null, iterations: 4);
                Cv2.Dilate(src: closed, dst: closed, element: null, iterations: 4);

                if (debug)
                {
                    //closed.CopyTo(result);
                }

                // 5. Find the contours in the thresholded image. Sort the contours by their area
                Point[][] contours;
                HierarchyIndex[] hierarchyIndices;
                Cv2.FindContours(
                    image: closed,
                    contours: out contours,
                    hierarchy: out hierarchyIndices,
                    mode: RetrievalModes.External,
                    method: ContourApproximationModes.ApproxSimple);

                if (contours.Length > 0)
                {
                    // Sort contour area by Linq method
                    // Contour with the largest area is stored in the first index
                    contours = contours.OrderByDescending(x => Cv2.ContourArea(x)).ToArray();
                    region = Cv2.BoundingRect(curve: contours[0]);

                    Cv2.Rectangle(img: mat, rect: region, color: new Scalar(0, 0, 255), thickness: 3);

                }
                mat.CopyTo(result);
            }

            return result;
        }

    }
}
