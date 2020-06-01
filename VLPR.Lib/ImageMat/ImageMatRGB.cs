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
            int H, V;
            double S;

            var max = Math.Max(Math.Max(R, G), B);
            var min = Math.Min(Math.Min(R, G), B);

            H = 0;
            if (max == min)
            {
                H = 0;
            }
            else if (max == R && G > B)
            {
                H = 60 * (G - B) / (max - min) + 0;
            }
            else if (max == R && G < B)
            {
                H = 60 * (G - B) / (max - min) + 360;
            }
            else if (max == G)
            {
                H = H = 60 * (B - R) / (max - min) + 120;
            }
            else if (max == B)
            {
                H = H = 60 * (R - G) / (max - min) + 240;
            }
            // S
            if (max == 0)
            {
                S = 0;
            }
            else
            {
                S = (max - min) * 1.0f / max;
            }
            // V
            V = max;


            result.H = H;
            result.S = S;
            result.V = (byte)V;
        }
    }
    public class ImageMatRGB:ImageMat<PixelRGB>
    {
        public enum PixelBytesFormat
        {
            RGB,
            BGR,
            RGBA,
            BGRA,
            ABGR
        }
        public ImageMatRGB(PixelRGB[] pixels, int width, int height, bool byClone = true)
            : base(pixels, width, height, byClone)
        {

        }

        public ImageMatRGB(byte[] pixels, int width, int height,PixelBytesFormat format)
        {
            switch (format)
            {
                case PixelBytesFormat.RGB:
                case PixelBytesFormat.BGR:
                    if (width * height > pixels.Length * 3)
                        throw new Exception("Array is too short!");
                    break;
                case PixelBytesFormat.RGBA:
                case PixelBytesFormat.ABGR:
                    if (width * height > pixels.Length * 4)
                        throw new Exception("Array is too short!");
                    break;
            }

            data = new PixelRGB[width * height];
            switch (format)
            {
                case PixelBytesFormat.RGB:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].R = pixels[i * 3];
                        data[i].G = pixels[i * 3 + 1];
                        data[i].B = pixels[i * 3 + 2];
                    }break;
                case PixelBytesFormat.BGR:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].B = pixels[i * 3];
                        data[i].G = pixels[i * 3 + 1];
                        data[i].R = pixels[i * 3 + 2];
                    }
                    break;
                case PixelBytesFormat.RGBA:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].R = pixels[i * 4];
                        data[i].G = pixels[i * 4 + 1];
                        data[i].B = pixels[i * 4 + 2];
                    }
                    break;
                case PixelBytesFormat.BGRA:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].B = pixels[i * 4];
                        data[i].G = pixels[i * 4 + 1];
                        data[i].R = pixels[i * 4 + 2];
                    }
                    break;
                case PixelBytesFormat.ABGR:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i].R = pixels[i * 4 + 3];
                        data[i].G = pixels[i * 4 + 2];
                        data[i].B = pixels[i * 4 + 1];
                    }
                    break;
            }

            Width = width;
            Height = height;
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

        protected override void OnAsRGB(int index, out PixelRGB pixelRGB)
        {
            pixelRGB = data[index];
        }
    }
}
