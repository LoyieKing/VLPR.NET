using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public struct PixelHSV
    {
		public int H;
        public double S;
		public byte V;

		public void HSVtoRGB(out PixelRGB pixelRGB)
		{
			// R,G,B from 0-255, H from 0-360, S,V from 0-100
			int i;
			double RGB_min, RGB_max;
			RGB_max = V;
			RGB_min = RGB_max * (1.0f - S);

			i = H / 60;
			int difS = H % 60; // factorial part of H

			// RGB adjuStment amount by Hue 
			double RGB_Adj = (RGB_max - RGB_min) * difS / 60.0f;

			switch (i)
			{
				case 0:
					pixelRGB.R = (byte)RGB_max;
					pixelRGB.G = (byte)(RGB_min + RGB_Adj);
					pixelRGB.B = (byte)RGB_min;
					break;
				case 1:
					pixelRGB.R = (byte)(RGB_max - RGB_Adj);
					pixelRGB.G = (byte)RGB_max;
					pixelRGB.B = (byte)RGB_min;
					break;
				case 2:
					pixelRGB.R = (byte)RGB_min;
					pixelRGB.G = (byte)RGB_max;
					pixelRGB.B = (byte)(RGB_min + RGB_Adj);
					break;
				case 3:
					pixelRGB.R = (byte)RGB_min;
					pixelRGB.G = (byte)(RGB_max - RGB_Adj);
					pixelRGB.B = (byte)RGB_max;
					break;
				case 4:
					pixelRGB.R = (byte)(RGB_min + RGB_Adj);
					pixelRGB.G = (byte)RGB_min;
					pixelRGB.B = (byte)RGB_max;
					break;
				default: //case 5:
					pixelRGB.R = (byte)RGB_max;
					pixelRGB.G = (byte)RGB_min;
					pixelRGB.B = (byte)(RGB_max - RGB_Adj);
					break;
			}
		}


	}
	public class ImageMatHSV:ImageMat<PixelHSV>
    {
        public ImageMatHSV(PixelHSV[] hsvs, int width, int height, bool byClone = true)
            :base(hsvs,width,height,byClone)
        {

        }

		protected override void OnAsRGB(int index, out PixelRGB pixelRGB)
		{
			data[index].HSVtoRGB(out pixelRGB);
		}
    }
}
