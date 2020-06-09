using System;
using System.Data.Common;
using System.Globalization;
using System.Runtime.InteropServices;

namespace VLPR.Lib
{
    public struct Point
    {
        public int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public struct PointDouble
    {
        public double X, Y;
        public PointDouble(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public abstract class ImageMat<T> where T : struct
    {
        public delegate void ForRangeAction(int x, int y, T pixel);
        public delegate bool ThresholdAction(int x, int y, T pixel);
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
        public int Width { get; protected set; }
        public int Height { get; protected set; }


        internal ImageMat() { }

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

        public ImageMat(int width, int height)
        {
            data = new T[width * height];

            Width = width;
            Height = height;
        }

        public ImageMat(ImageMat<T> image)
        {
            data = (T[])image.data.Clone();



            Width = image.Width;
            Height = image.Height;
        }


        protected abstract void OnAsRGB(int index,out PixelRGB pixelRGB);

        public ImageMatRGB ToImageMatRGB()
        {
            var rgbs = new PixelRGB[Width * Height];
            for (int i = 0; i < data.Length; i++)
                OnAsRGB(i, out rgbs[i]);

            ImageMatRGB result = new ImageMatRGB(rgbs, Width, Height, false);
            return result;
        }

        public ImageMatBlackWhite ToImageBlackWhite(ThresholdAction threshold)
        {
            var bools = new bool[Width * Height];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    bools[i * Width + j] = threshold(j, i, data[i * Width + j]);
                }
            }
            ImageMatBlackWhite result = new ImageMatBlackWhite(bools, Width, Height, false);
            return result;
        }

        public byte[] ToBGRBytes()
        {
            byte[] result = new byte[data.Length * 3];
            for (int i = 0; i < data.Length; i++)
            {
                OnAsRGB(i, out var rgb);
                result[i * 3] = rgb.B;
                result[i * 3 + 1] = rgb.G;
                result[i * 3 + 2] = rgb.R;
            }
            return result;
        }

        public void ForRange(int left, int top, int right, int bottom, ForRangeAction action)
        {
            if (left > right || top > bottom)
                throw new ArgumentException("range not right!");
            for (int i = top; i <= bottom; i++)
            {
                int tmp = i * Width;
                for (int j = left; j <= right; j++)
                {
                    action(j, i, data[tmp + j]);
                }
            }
        }

        public MatLine this[int y]
        {
            get => new MatLine(this, y);
        }



    }
}
