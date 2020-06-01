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

        public ImageMatBlackWhite Dilation()
        {
            var bools = new bool[data.Length];
            for (int i = 1; i < Height - 1; i++)
            {
                for (int j = 1; j < Width - 1; j++)
                {
                    if (!data[Index(j, i)])
                        continue;

                    for (int a = -1; a <= 1; a++)
                    {
                        for (int b = -1; b <= -1; b++)
                        {
                            bools[Index(j + a, i + b)] = true;
                        }
                    }
                }
            }

            return new ImageMatBlackWhite(bools, Width, Height, false);
        }

        public ImageMatBlackWhite Erode()
        {
            var bools = new bool[data.Length];
            for (int i = 1; i < Height - 1; i++)
            {
                for (int j = 1; j < Width - 1; j++)
                {
                    bool flag = true;

                    for (int a = -1; a <= 1; a++)
                    {
                        for (int b = -1; b <= -1; b++)
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

        public ImageMatBlackWhite Opening()
        {
            return Erode().Dilation();
        }

        public ImageMatBlackWhite Closing()
        {
            return Dilation().Erode();
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
