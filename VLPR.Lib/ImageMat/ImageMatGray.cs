using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public class ImageMatGray : ImageMat<byte>
    {
        public ImageMatGray(byte[] grayData, int width, int height, bool byClone = true)
            : base(grayData, width, height, byClone)
        {

        }

        public ImageMatGray(int width, int height)
            : base(width, height)
        {

        }

        public ImageMatBlackWhite ToImageBlackWhite(int threshold)
        {
            var bools = new bool[Width * Height];
            for (int i = 0; i < data.Length; i++)
            {
                bools[i] = data[i] > threshold ? true : false;
            }

            ImageMatBlackWhite result = new ImageMatBlackWhite(bools, Width, Height, false);
            return result;
        }

        protected override void OnAsRGB(int index, out PixelRGB pixelRGB)
        {
            pixelRGB.R = pixelRGB.G = pixelRGB.B = data[index];
        }
    }
}
