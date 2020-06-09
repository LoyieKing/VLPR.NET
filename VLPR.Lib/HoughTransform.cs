using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VLPR.Lib
{
    public class HoughTransform
    {
        public struct Line
        {
            public readonly double Theta;
            public readonly double Distance;

            public Line(double theta, double dis)
            {
                Theta = theta;
                Distance = dis;
            }
        }
        private struct HoughPixel
        {
            public int counter;
            public int minx, miny;
            public int maxx, maxy;

            public void Update(int x, int y)
            {
                counter++;
                if (x < minx || y < miny)
                {
                    minx = x;
                    miny = y;
                }
                if (x > maxx || x > maxy)
                {
                    maxx = x;
                    maxy = y;
                }
            }

            public int Length => (int)Math.Sqrt((maxx - minx) * (maxx - minx) + (maxy - miny) * (maxy - miny));
        }

        ImageMatBlackWhite image;
        int scale;
        HoughPixel[,] hough_space;
        double ttheta;
        int maxd;

        public HoughTransform(ImageMatBlackWhite pic, int scale)
        {
            this.image = pic;
            this.scale = scale;


            /*(x,y) => Hough-Space (θ,d)
             * θ：the angle between x-axis and the line which passing through (x,y)
             * d : the distance from origin to the line which passing through (x,y)
             * 
             * 
             * so, mathematically,
             * d = xcosθ + ysinθ
             * by The auxiliary Angle formula(辅助角公式),
             * max(d) = sqrt(x²+y²)
             * so d:[-sqrt(x²+y²),sqrt(x²+y²)]
            */


            double maxr = Math.Sqrt((pic.Width * pic.Width + pic.Height * pic.Height));
            //we need to create a array two times big because d:[-sqrt(x²+y²),sqrt(x²+y²)]

            maxd = (int)Math.Ceiling(maxr) + 1;
            hough_space = new HoughPixel[maxd * 2, scale + 1];
            for (int i = 0; i < maxd * 2; i++)
            {
                for (int j = 0; j < scale + 1; j++)
                {
                    hough_space[i, j].minx = int.MaxValue;
                    hough_space[i, j].miny = int.MaxValue;
                    hough_space[i, j].maxx = int.MinValue;
                    hough_space[i, j].maxy = int.MinValue;
                }
            }

            /*
             * maxd*2 because d:[-sqrt(x²+y²),sqrt(x²+y²)] => d:[-maxd,maxd]
             * scale+1 because 0<=i<=scale
             */

            //θ=π/scale*x,so save the tmpθ to accelerate calcaltion /
            ttheta = Math.PI / scale;
        }

        public void Calculate()
        {
            image.ForEach((x, y, pixel) => {
                if (!pixel)
                    return;

                for (int i = 0; i <= scale; i++)
                {
                    double theta = ttheta * i;
                    int d = (int)(x * Math.Cos(theta) + y * Math.Sin(theta));
                    hough_space[d + maxd, i].Update(x, y);
                }
            });
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
        private Dictionary<int, List<Point>> SearchConnectedRegion(int treshold,int connected_rangex = 1, int connected_rangey = 1)
        {
            int Width = scale + 1;
            int Height = maxd * 2;

            int Index(int x, int y)
            {
                return y * (scale + 1) + x;
            }

            Dictionary<int, List<Point>> result = new Dictionary<int, List<Point>>();

            us_father = new int[hough_space.Length];
            for (int i = 0; i < us_father.Length; i++)
                us_father[i] = i;

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (hough_space[i, j].counter < treshold)
                        continue;

                    var index = Index(j, i);


                    for (int a = -connected_rangex; a <= connected_rangex; a++)
                    {
                        for (int b = -connected_rangey; b <= connected_rangey; b++)
                        {
                            if (a == 0 && b == 0)
                                continue;
                                    
                            int x = (j + a + Width) % Width;
                            int y = (i + b + Height) % Height;
                            if (j + a < 0 || j + a >= Width)
                                y = Height - y;
                            var t = Index(x, y);
                            if (hough_space[y, x].counter >= treshold)
                                us_union(index, t);


                        }
                    }

                }
            }

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    int index = Index(j, i);

                    if (hough_space[i, j].counter < treshold)
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

        private string X2D(int x)
        {
            return "" + 180.0 / scale * x + "°";
        }

        public List<Line> SelectLines(int noiseThreshold)
        {
            List<Line> result = new List<Line>();



            var regions = SearchConnectedRegion(noiseThreshold, scale * 45 / 180, 10).Values.ToList();
            regions.Sort((a, b) => b.Count - a.Count);

            var sregions = regions.Take(4);

            var avgPoints = new List<string>();

            foreach (var ps in sregions)
            {
                Point avgPoint;
                avgPoint.x = 0;
                avgPoint.y = 0;
                foreach (var p in ps)
                {
                    if (p.y >= maxd)
                    {
                        avgPoint.x += p.x;
                        avgPoint.y += p.y;
                    }
                    else
                    {
                        avgPoint.x += p.x - scale;
                        avgPoint.y += 2 * maxd - p.y;
                    }


                }
                avgPoint.x /= ps.Count;
                avgPoint.y /= ps.Count;

                

                double theta = ttheta * avgPoint.x;
                result.Add(new Line(theta, avgPoint.y - maxd));


                avgPoints.Add(X2D(avgPoint.x) + " " + (avgPoint.y - maxd));
            }
            

            return result;
            //for (int i = 0; i < maxd * 2; i++)
            //{
            //    for (int j = 0; j <= scale; j++)
            //    {
            //        if (hough_space[i, j].counter < noiseThreshold)
            //            continue;



            //    }
            //}


            //List<Line> denoise = new List<Line>();
            //for (int i = 0; i < maxd * 2; i++)
            //{
            //    for (int j = 0; j <= scale; j++)
            //    {
            //        if (hough_space[i, j].counter < noiseThreshold)
            //            continue;

            //        double theta = ttheta * j;

            //        denoise.Add(new Line(theta, i - maxd, hough_space[i, j].Length));
            //    }
            //}

            //return denoise;
        }
        


    }
}
