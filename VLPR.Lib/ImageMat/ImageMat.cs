using System;
using System.Data.Common;
using System.Globalization;

namespace VLPR.Lib
{

    public class ImageMat<T> where T : struct
    {
        public struct MatLine
        {
            private ImageMat<T> imageMat;
            private int line;
            internal MatLine(ImageMat<T> imageMat, int line)
            {
                this.imageMat = imageMat;
                this.line = line;
            }

            public ref T this[int x]
            {
                get => ref imageMat.data[line * imageMat.Width + x];
            }
        }


        public enum Type
        {
            RGB,
            Gray
        }

        protected T[] data;
        public int Width { get; }
        public int Height { get; }

        public ImageMat(T[] data, int width, int height, bool cloneArray = true)
        {
            if (data.Length < width * height)
                throw new Exception("Array is too short!");

            int len = width * height;
            if (cloneArray)
                this.data = (T[])data.Clone();
            else
                this.data = data;

            Width = width;
            Height = height;
        }

        public ImageMat(ImageMat<T> image)
        {
            data = (T[])image.data.Clone();



            Width = image.Width;
            Height = image.Height;
        }

        public MatLine this[int y]
        {
            get => new MatLine(this, y);
        }

    }
}
