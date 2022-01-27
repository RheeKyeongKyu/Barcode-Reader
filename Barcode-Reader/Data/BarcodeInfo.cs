using OpenCvSharp;
using System.Drawing;

namespace Barcode_Reader
{
    public class BarcodeInfo
    {
        public Bitmap ProcessedImage { get; set; } = null;
        public Rect RegionRect { get; set; } = new Rect();
        public bool BarcodeFound { get; set; } = false;
        public Bitmap BarcodeImage { get; set; } = null;
        public Bitmap BarcodeDecodeResult { get; set; } = null;

        public BarcodeInfo(Bitmap processedImage, Rect regionRect, bool barcodeFound, Bitmap barcodeImage, Bitmap barcodeDecodeResult)
        {
            ProcessedImage = processedImage;
            RegionRect = regionRect;
            BarcodeFound = barcodeFound;
            BarcodeImage = barcodeImage;
            BarcodeDecodeResult = barcodeDecodeResult;
        }
    }
}
