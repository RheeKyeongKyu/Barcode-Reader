using OpenCvSharp;
using System.Drawing;

namespace Barcode_Reader
{
    public class BarcodeInfo
    {
        public Rect Rect { get; set; } = new Rect();
        public bool BarcodeFound { get; set; } = false;
        public Bitmap BarcodeImage { get; set; } = null;
        public Bitmap BarcodeDecodeResult { get; set; } = null;

        public BarcodeInfo(Rect rect, bool barcodeFound, Bitmap barcodeImage, Bitmap barcodeDecodeResult)
        {
            Rect = rect;
            BarcodeFound = barcodeFound;
            BarcodeImage = barcodeImage;
            BarcodeDecodeResult = barcodeDecodeResult;
        }
    }
}
