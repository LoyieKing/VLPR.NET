using System;
using System.Collections.Generic;
using System.Text;

namespace VLPR.Lib
{
    public static class InversePerspectiveHelper<T,T2>
        where T:ImageMat<T2>
        where T2:struct
    {
        public static T InversePerspective(T bitmap, List<HoughTransform.Line> lines)
        {

            var p01 = ret2linesP(lines[0], lines[1]);
            var p02 = ret2linesP(lines[0], lines[2]);
            var p03 = ret2linesP(lines[0], lines[3]);
            var p12 = ret2linesP(lines[1], lines[2]);
            var p13 = ret2linesP(lines[1], lines[3]);
            var p23 = ret2linesP(lines[2], lines[3]);

            List<PointDouble> p4s = new List<PointDouble>();
            if (isokp(p01, bitmap)
                && isokp(p02, bitmap)
                && isokp(p13, bitmap)
                && isokp(p23, bitmap)
                && !(isinside(p01, p02, p13, p23, p03) || isinside(p01, p02, p13, p23, p12)))
            {
                p4s.Add(p01);
                p4s.Add(p02);
                p4s.Add(p13);
                p4s.Add(p23);
            }

            if (isokp(p01, bitmap)
                && isokp(p03, bitmap)
                && isokp(p12, bitmap)
                && isokp(p23, bitmap)
                && !(isinside(p01, p03, p12, p23, p02) || isinside(p01, p03, p12, p23, p13)))
            {
                p4s.Add(p01);
                p4s.Add(p03);
                p4s.Add(p12);
                p4s.Add(p23);
            }

            if (isokp(p02, bitmap)
                && isokp(p03, bitmap)
                && isokp(p12, bitmap)
                && isokp(p13, bitmap)
                && !(isinside(p02, p03, p12, p13, p01) || isinside(p02, p03, p12, p13, p23)))
            {
                p4s.Add(p02);
                p4s.Add(p03);
                p4s.Add(p12);
                p4s.Add(p13);
            }
            double minx = bitmap.Width + 2;
            foreach (var p in p4s)
            {
                if (p.X < minx)
                    minx = p.X;
            }
            double miny = bitmap.Height + 2;
            foreach (var p in p4s)
            {
                if (p.Y < miny)
                    miny = p.Y;
            }
            p4s.Sort(
                (A, B) =>
                {
                    double disa = (A.X - minx) * (A.X - minx) + (A.Y - miny) * (A.Y - miny);
                    double disb = (B.X - minx) * (B.X - minx) + (B.Y - miny) * (B.Y - miny);
                    if (disa > disb)
                        return 1;
                    else
                        return -1;
                }
                );

            //短边
            var AB = Math.Sqrt((p4s[0].X - p4s[1].X) * (p4s[0].X - p4s[1].X) + (p4s[0].Y - p4s[1].Y) * (p4s[0].Y - p4s[1].Y));
            var CD = Math.Sqrt((p4s[3].X - p4s[2].X) * (p4s[3].X - p4s[2].X) + (p4s[3].Y - p4s[2].Y) * (p4s[3].Y - p4s[2].Y));
            //长边
            var AC = Math.Sqrt((p4s[0].X - p4s[3].X) * (p4s[0].X - p4s[3].X) + (p4s[0].Y - p4s[3].Y) * (p4s[0].Y - p4s[3].Y));
            var BD = Math.Sqrt((p4s[1].X - p4s[2].X) * (p4s[1].X - p4s[2].X) + (p4s[1].Y - p4s[2].Y) * (p4s[1].Y - p4s[2].Y));

            var imgw = 440;
            var imgh = 140;

            var img = (T)Activator.CreateInstance(bitmap.GetType(), new object[] { imgw, imgh });

            PointDouble calPoint(int p1, int p2,int j,int i)
            {
                PointDouble result;
                result.X = p4s[p1].X + (p4s[p2].X - p4s[p1].X) * (i + 1) / (imgw - 1);
                result.Y = p4s[p1].Y + (p4s[p2].Y - p4s[p1].Y) * (j + 1) / (imgh - 1);
                return result;
            }
            (double k, double b) calLine(PointDouble p1, PointDouble p2)
            {
                double k = (p1.Y - p2.Y) / (p1.X - p2.X);
                double b = p2.Y - (p1.Y - p2.Y) * p2.X / (p1.X - p2.X);
                return (k, b);      
            }


            for (int j = 0; j < imgh; j++)
            {
                for (int i = 0; i < imgw; i++)
                {
                    PointDouble pac = calPoint(0, 2, j, i);
                    PointDouble pbd = calPoint(1, 3, j, i);
                    (double k1, double b1) = calLine(pac, pbd);


                    PointDouble pab = calPoint(0, 1, j, i);
                    PointDouble pcd = calPoint(2, 3, j, i);
                    (double k2, double b2) = calLine(pab, pcd);

                    PointDouble aimPoint;
                    aimPoint.X = (b2 - b1) / (k1 - k2);
                    aimPoint.Y = k1 * aimPoint.X + b1;

                    img[j, i] = bitmap[(int)aimPoint.Y, (int)aimPoint.X];
                }
            }

            return img;
        }



        private static PointDouble ret2linesP(HoughTransform.Line a, HoughTransform.Line b)
        {
            var P = new PointDouble();

            var A1 = Math.Cos(a.Theta);
            var B1 = Math.Sin(a.Theta);
            var C1 = -a.Distance;

            var A2 = Math.Cos(b.Theta);
            var B2 = Math.Sin(b.Theta);
            var C2 = -b.Distance;

            var x = (B1 * C2 - B2 * C1) / (A1 * B2 - A2 * B1);
            var y = (A1 * C2 - A2 * C1) / (A2 * B1 - A1 * B2);

            P.X = x;
            P.Y = y;
            return P;
        }
        private static bool isokp(PointDouble p, T bmp)
        {
            double width = bmp.Width;
            double height = bmp.Height;

            if (p.X < 0 || p.X > width)
                return false;

            if (p.Y < 0 || p.Y > height)
                return false;

            return true;
        }
        private static bool isinside(PointDouble A, PointDouble B, PointDouble C, PointDouble D, PointDouble P)
        {
            double a = (B.X - A.X) * (P.Y - A.Y) - (B.Y - A.Y) * (P.X - A.X);
            double b = (C.X - B.X) * (P.Y - B.Y) - (C.Y - B.Y) * (P.X - B.X);
            double c = (D.X - C.X) * (P.Y - C.Y) - (D.Y - C.Y) * (P.X - C.X);
            double d = (A.X - D.X) * (P.Y - D.Y) - (A.Y - D.Y) * (P.X - D.X);
            if ((a > 0 && b > 0 && c > 0 && d > 0) || (a < 0 && b < 0 && c < 0 && d < 0))
            {
                return true;
            }

            //      AB X AP = (b.x - a.x, b.y - a.y) x (p.x - a.x, p.y - a.y) = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
            //      BC X BP = (c.x - b.x, c.y - b.y) x (p.x - b.x, p.y - b.y) = (c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x);
            return false;

        }



    }
}
