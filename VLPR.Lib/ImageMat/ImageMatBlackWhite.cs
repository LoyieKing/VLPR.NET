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

        public ImageMatBlackWhite(int width,int height)
            :base(width,height)
        {

        }

        protected override void OnAsRGB(int index, out PixelRGB pixelRGB)
        {
            if (data[index])
                pixelRGB.R = pixelRGB.G = pixelRGB.B = 255;
            else
                pixelRGB.R = pixelRGB.G = pixelRGB.B = 0;
        }


        private int Index(int x, int y)
        {
            return y * Width + x;
        }

        public ImageMatBlackWhite Dilation(int range = 1)
        {
            bool[] bools = new bool[data.Length];

            for (int i = range; i < Height - range; i++)
            {
                for (int j = range; j < Width - range; j++)
                {
                    if (!data[Index(j, i)])
                        continue;

                    for (int a = -range; a <= range; a++)
                    {
                        for (int b = -range; b <= -range; b++)
                        {
                            bools[Index(j + a, i + b)] = true;
                        }
                    }
                }
            }

            return new ImageMatBlackWhite(bools, Width, Height, false);
        }

        public ImageMatBlackWhite Erode(int range = 1)
        {
            bool[] bools = new bool[data.Length];

            for (int i = range; i < Height - range; i++)
            {
                for (int j = range; j < Width - range; j++)
                {
                    bool flag = true;

                    for (int a = -range; a <= range; a++)
                    {
                        for (int b = -range; b <= -range; b++)
                        {
                            if(!data[Index(j + a, i + b)])
                            {
                                flag = false;
                                goto BREAK;
                            }
                        }
                    }

                BREAK:;
                    bools[Index(j, i)] = flag;
                }
            }

            return new ImageMatBlackWhite(bools, Width, Height, false);
        }

        public ImageMatBlackWhite Opening(int range = 1)
        {
            return Erode(range).Dilation(range);
        }

        public ImageMatBlackWhite Closing(int range = 1)
        {
            return Dilation(range).Erode(range);
        }

        private int[] us_father;
        private int us_find(int me)
        {
            int root = us_father[me];
            while (us_father[root] != root)
                root = us_father[root];

            while (us_father[me] != root)
            {
                int t = us_father[me];
                us_father[me] = root;
                me = t;
            }

            return root;
        }

        private void us_union(int a, int b)
        {
            int f1 = us_find(a);
            int f2 = us_find(b);
            us_father[f2] = f1;
        }
        public Dictionary<int, List<Point>> SearchConnectedRegion(int connected_range = 1)
        {
            Dictionary<int, List<Point>> result = new Dictionary<int, List<Point>>();

            us_father = new int[data.Length];
            for (int i = 0; i < data.Length; i++)
                us_father[i] = i;

            for (int i = connected_range; i < Height - connected_range; i++)
            {
                for (int j = connected_range; j < Width - connected_range; j++)
                {
                    var index = Index(j, i);

                    if (!data[index])
                        continue;


                    for (int a = -connected_range; a <= connected_range; a++)
                    {
                        for (int b = -connected_range; b <= connected_range; b++)
                        {
                            if (a == 0 && b == 0)
                                continue;

                            var t = Index(j + a, i + b);
                            if (data[t])
                                us_union(index, t);
                        }
                    }

                    if (data[Index(j - 1, i - 1)])
                        us_union(Index(j, i), Index(j - 1, i - 1));
                }
            }

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    int index = Index(j, i);

                    if (!data[index])
                        continue;

                    int fa = us_father[index];
                    Point me = new Point(j, i);

                    if (result.ContainsKey(fa))
                    {
                        result[fa].Add(me);
                    }
                    else
                    {
                        result.Add(fa, new List<Point>() { me });
                    }
                }
            }


            return result;
        }
    }
}
