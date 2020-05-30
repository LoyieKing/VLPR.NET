using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public struct PixelRGB
    {
        public byte R, G, B;

        public byte ToPixelGray()
        {
            return (byte)(((int)R * 30 + (int)G * 59 + (int)B * 11 + 50) / 100);
        }

        public void ToPixelHSV(out PixelHSV result)
        {
            int H, S, V;
            H = 0;
            var max = Math.Max(Math.Max(R, G), B);
            var min = Math.Min(Math.Min(R, G), B);

            S = (max - min) / max;
            V = max;

            if (R == max)
            {
                H = (G - B) / (max - min);
            }
            if (G == max)
            {
                H = 2 + (B - R) / (max - min);
            }
            if (B == max)
            {
                H = 4 + (R - G) / (max - min);
            }
            H = (H / 6);
            if (H < 0)
            {
                H = (H / 360 + 1);
            }

            result.H = (byte)H;
            result.S = (byte)S;
            result.V = (byte)V;
        }
    }
    class ImageMatRGB:ImageMat<PixelRGB>
    {
        public ImageMatRGB(PixelRGB[] pixels, int width, int height, bool byClone = true)
            : base(pixels, width, height, byClone)
        {

        }

        public ImageMatBlackWhite ToImageBlackWhite(int threshold)
        {
            var bools = new bool[Width * Height];
            for (int i = 0; i < data.Length; i++)
            {
                bools[i] = data[i].ToPixelGray() > threshold ? true : false;
            }

            ImageMatBlackWhite result = new ImageMatBlackWhite(bools, Width, Height, false);
            return result;
        }

        public ImageMatGray ToImageGray()
        {
            var bytes = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                bytes[i] = data[i].ToPixelGray();
            }

            ImageMatGray result = new ImageMatGray(bytes, Width, Height, false);
            return result;
        }

        public ImageMatHSV ToImageHSV()
        {
            var hsvs = new PixelHSV[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i].ToPixelHSV(out hsvs[i]);
            }

            ImageMatHSV result = new ImageMatHSV(hsvs, Width, Height, false);
            return result;
        }
        
    }
}
