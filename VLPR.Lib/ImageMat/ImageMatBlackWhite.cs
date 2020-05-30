using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public class ImageMatBlackWhite:ImageMat<bool>
    {
        public ImageMatBlackWhite(bool[] data, int width, int height, bool byClone = true)
            : base(data, width, height, byClone)
        {

        }
    }
}
