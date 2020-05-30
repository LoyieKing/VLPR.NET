using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public struct PixelHSV
    {
        public byte H, S, V;
    }
    public class ImageMatHSV:ImageMat<PixelHSV>
    {
        public ImageMatHSV(PixelHSV[] hsvs, int width, int height, bool byClone = true)
            :base(hsvs,width,height,byClone)
        {

        }
    }
}
