using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Linq;
using ZXing;
using ZXing.Common;

namespace Barcode_Reader
{
    public static class ImageProcessing
    {
        public static BarcodeInfo GetBarcodeRegion(Mat input, bool debug = false)
        {
            // Assign default values to properties of BarcodeInfo object
            Mat processedImageMat = new Mat(); // processedImageMat will be converted to Bitmap at the end of method
            Rect regionRect = new Rect();
            bool barcodeFound = false;
            System.Drawing.Bitmap barcodeImage = null;
            System.Drawing.Bitmap barcodeDecodeResult = null;

            // Barcode region candidate
            Rect barcodeRectCandidate = new Rect();

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
                if (input.Channels() > 1)
                    Cv2.CvtColor(src: input, dst: gray, ColorConversionCodes.BGR2GRAY);
                else
                    input.CopyTo(gray);

                if (debug)
                {
                    //gray.CopyTo(processedImageMat);
                }

                // 2. Compute the Scharr gradient magnitude representation of the image in both X and Y direction
                Cv2.Sobel(src: gray, dst: gradX, ddepth: MatType.CV_32F, xorder: 1, yorder: 0, ksize: -1);
                Cv2.Sobel(src: gray, dst: gradY, ddepth: MatType.CV_32F, xorder: 0, yorder: 1, ksize: -1);

                // subtract the y-gradient from the x-gradient
                Cv2.Subtract(src1: gradX, src2: gradY, dst: gradient);
                Cv2.ConvertScaleAbs(src: gradient, dst: gradient);

                if (debug)
                {
                    //gradient.CopyTo(processedImageMat);
                }

                // 3. Blur and threshold the image
                Cv2.Blur(src: gradient, dst: blurred, ksize: new Size(9, 9));
                Cv2.Threshold(src: blurred, dst: thresholded, thresh: 190, maxval: 255, type: ThresholdTypes.Binary);

                if (debug)
                {
                    //thresholded.CopyTo(processedImageMat);
                }

                // 4. Construct a closing kernel and apply it to the thresholded image
                Cv2.MorphologyEx(src: thresholded, dst: closed, op: MorphTypes.Close, element: kernel);

                // perform a series of erosions and dilations
                Cv2.Erode(src: closed, dst: closed, element: null, iterations: 4);
                Cv2.Dilate(src: closed, dst: closed, element: null, iterations: 4);

                if (debug)
                {
                    //closed.CopyTo(processedImageMat);
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
                    barcodeRectCandidate = Cv2.BoundingRect(curve: contours[0]);

                    // Crop the barcode area from the grayscaled image
                    using (Mat barcodeRegion = new Mat(m: gray, roi: barcodeRectCandidate))
                    
                    // 6. Add Whitespace around barcode
                    using (Mat barcodeRegionFinal = GetBarcodeContainer(barcodeRegion: barcodeRegion, border: 15))
                    {
                        // 7. Recognize the barcode using ZXing library
                        if (DecodeBarCode(barcodeCandidate: barcodeRegionFinal.ToBitmap(),
                                        result: out Result decodeResult))
                        {
                            // Draw green rectangle around barcode region and show recognized barcode text above the green box
                            Cv2.Rectangle(img: input, rect: barcodeRectCandidate, color: new Scalar(0, 255, 0), thickness: 3);
                            Cv2.PutText(img: input, text: $"{decodeResult.Text} ({decodeResult.BarcodeFormat})", org: new Point(barcodeRectCandidate.Left, barcodeRectCandidate.Top), fontFace: HersheyFonts.HersheyPlain, fontScale: 3, color: new Scalar(0, 255, 0), thickness: 3);

                            // Render barcode
                            BarcodeWriter writer = new BarcodeWriter
                            {
                                Format = decodeResult.BarcodeFormat,
                                Options = { Width = 400, Height = 100, Margin = 4 },
                                Renderer = new ZXing.Rendering.BitmapRenderer()
                            };

                            // reassign value to out parameters
                            regionRect = barcodeRectCandidate;
                            barcodeFound = true;
                            barcodeImage = barcodeRegion.ToBitmap();
                            barcodeDecodeResult = writer.Write(decodeResult.Text);
                        }
                        else
                        {
                            // Draw red rectangle around barcode region candidate
                            Cv2.Rectangle(img: input, rect: barcodeRectCandidate, color: new Scalar(0, 0, 255), thickness: 3);
                        }
                    }
                }
                input.CopyTo(processedImageMat);
            }

            return new BarcodeInfo(processedImage: processedImageMat.ToBitmap(), 
                                regionRect: regionRect, 
                                barcodeFound: barcodeFound, 
                                barcodeImage: barcodeImage, 
                                barcodeDecodeResult: barcodeDecodeResult);
        }

        // Decode barcode using ZXing library
        private static bool DecodeBarCode(System.Drawing.Bitmap barcodeCandidate, out Result result)
        {
            result = null;

            bool barcodeFound = false;

            try
            {
                LuminanceSource source = new BitmapLuminanceSource(barcodeCandidate);
                BarcodeReader reader = new BarcodeReader(reader: null, createLuminanceSource: null, createBinarizer: ls => new GlobalHistogramBinarizer(ls))
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true
                        //,TryInverted = true  // Commented because of IndexOutOfRangeException in reader.Decode(source)
                    }
                };

                result = reader.Decode(source);

                // Successfully decoded
                if (result != null)
                {
                    barcodeFound = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return barcodeFound;
        }

        // Barcode region is padded in all 4 sides with specifed white pixels
        private static Mat GetBarcodeContainer(Mat barcodeRegion, int border)
        {
            // ZXing.Net requires white space around the barcode
            Mat barcodeContainer = new Mat(size: new Size(barcodeRegion.Width + (border * 2), barcodeRegion.Height + (border * 2)), type: MatType.CV_8U, s: Scalar.White);
            Rect barcodeRect = new Rect(location: new Point(border, border), size: new Size(barcodeRegion.Width, barcodeRegion.Height));
            using (Mat roi = barcodeContainer[barcodeRect])
            {
                barcodeRegion.CopyTo(roi);
            }

            return barcodeContainer;
        }
    }
}
