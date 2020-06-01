using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VLPR.Lib;

namespace VLPR.Desktop
{
    public static class VLPRExtension
    {
        public static WriteableBitmap ToWriteableBitmap<T>(this ImageMat<T> image) where T : struct
        {
            WriteableBitmap bitmap = new WriteableBitmap(image.Width, image.Height, 96, 96, PixelFormats.Bgr24, null);
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, image.Width, image.Height), image.ToBGRBytes(), image.Width *3, 0);
            return bitmap;
        }
    }
}
