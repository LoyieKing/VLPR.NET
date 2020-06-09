using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VLPR.Lib;

namespace VLPR.Desktop
{



    public partial class MainWindow : Window
    {
        ImageMatRGB imageRGB;
        ImageMatHSV imageHSV;
        ImageMatBlackWhite imageBW;
        BitmapImage bitmap;


        SortedDictionary<byte, int> Hs = new SortedDictionary<byte, int>();
        SortedDictionary<double, int> Ss = new SortedDictionary<double, int>();
        SortedDictionary<byte, int> Vs = new SortedDictionary<byte, int>();


        public MainWindow()
        {
            InitializeComponent();

        }

        private void ShowImage<T>(ImageMat<T> imageMat)
            where T : struct
        {
            image.Height = imageMat.Height;
            image.Width = imageMat.Width;
            image.Source = imageMat.ToWriteableBitmap();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\沪KR9888.png"));
            //bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\苏B79999.jpg"));
            //bitmap = new BitmapImage(new Uri(@"D:\Users\zzq\Desktop\PIC\沪KR9888.png"));
            bitmap = new BitmapImage(new Uri(@"D:\Users\zzq\Desktop\PIC\京N8P8F8.jpg"));
            //bitmap = new BitmapImage(new Uri(@"D:\Users\zzq\Desktop\PIC\贵C99999.jpg"));

            //image.Source = bitmap;



            var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            var stride = bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8;
            bitmap.CopyPixels(bytes, stride, 0);

            imageRGB = new ImageMatRGB(bytes, bitmap.PixelWidth, bitmap.PixelHeight, ImageMatRGB.PixelBytesFormat.BGRA);
            imageHSV = imageRGB.ToImageHSV();

            //image.Source = imageRGB.ToWriteableBitmap();

            var bwImage = imageHSV.ToImageBlackWhite((x, y, p) => p.H >= 200 && p.S >= 0.90 && p.V >= 100).Closing().Opening();
            var raw_regions = bwImage.SearchConnectedRegion(8);
            var erase_regions = raw_regions.Where(kv => kv.Value.Count < 1000).Select(kv => kv.Value);
            var save_regions = raw_regions.Where(kv => kv.Value.Count > 1000).Select(kv => kv.Value);
            //foreach (var region in erase_regions)
            //{
            //    foreach (var p in region)
            //    {
            //        bwImage[p.y][p.x] = false;
            //    }
            //}
            bwImage = bwImage.ToImageBlackWhite((x, y, p) =>
            {
                if (!p)
                    return false;

                bool flag = false;


                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (!bwImage[y + j][x + i])
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                return flag;
            });
            var bwImage2 = bwImage.ToImageBlackWhite((x, y, p) => false);
            for (int i = 0; i < bwImage.Width; i++)
            {
                for (int j = 0; j < bwImage.Height; j++)
                {
                    if (bwImage[j, i])
                    {
                        bwImage2[j, i] = true;
                        break;
                    }
                }
                for (int j = bwImage.Height - 1; j >= 0; j--)
                {
                    if (bwImage[j, i])
                    {
                        bwImage2[j, i] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < bwImage.Height; i++)
            {
                for (int j = 0; j < bwImage.Width; j++)
                {
                    if (bwImage[i, j])
                    {
                        bwImage2[i, j] = true;
                        break;
                    }
                }
                for (int j = bwImage.Width - 1; j >= 0; j--)
                {
                    if (bwImage[i, j])
                    {
                        bwImage2[i, j] = true;
                        break;
                    }
                }
            }

            bwImage = bwImage2;

            //image.Width = bitmap.PixelWidth;
            //image.Height = bitmap.PixelHeight;
            //image.Source = bwImage.ToWriteableBitmap();

            //return;
            HoughTransform houghTransform = new HoughTransform(bwImage, 500);
            houghTransform.Calculate();
            //ShowImage(houghTransform.SelectLines(40));


            var lines = houghTransform.SelectLines(30);
            //lines.Sort((a, b) => (int)(a.Distance - b.Distance));
            

            var showImage = imageRGB;
            foreach (var line in lines)
            {
                double cos = Math.Cos(line.Theta);
                double sin = Math.Sin(line.Theta);
                for (int i = 0; i < bwImage.Width; i++)
                {
                    int y = (int)((line.Distance - i * cos) / sin);
                    //Debug.WriteLine($"x:{i},y:{y}");
                    if (y >= bwImage.Height || y < 0)
                        continue;
                    showImage[y, i].R = 255;
                    showImage[y, i].G = 0;
                    showImage[y, i].B = 0;
                }

                for (int i = 0; i < bwImage.Height; i++)
                {
                    int x = (int)((line.Distance - i * sin) / cos);
                    if (x >= bwImage.Width || x < 0)
                        continue;
                    showImage[i, x].R = 255;
                    showImage[i, x].G = 0;
                    showImage[i, x].B = 0;
                }
            }
            ShowImage(showImage);
            //image.Source = showImage.ToWriteableBitmap();
            //var k = bwImage.ToImageMatRGB();
            //image_HoughTransform(imageBW, k);
            //image.Source = k.ToWriteableBitmap();

            var p01 = ret2linesP(lines[0], lines[1]);
            var p02 = ret2linesP(lines[0], lines[2]);
            var p03 = ret2linesP(lines[0], lines[3]);
            var p12 = ret2linesP(lines[1], lines[2]);
            var p13 = ret2linesP(lines[1], lines[3]);
            var p23 = ret2linesP(lines[2], lines[3]);

            List<System.Windows.Point> p4s = new List<System.Windows.Point>();
            if (isokp(p01, bitmap) 
                && isokp(p02, bitmap)
                && isokp(p13, bitmap)
                && isokp(p23, bitmap)
                && !(isinside(p01,p02,p13,p23,p03)|| isinside(p01,p02,p13,p23,p12))  )
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
            double minx = bitmap.PixelWidth + 2;
            foreach(var p in p4s)
            {
                if (p.X < minx) 
                    minx = p.X;
            }
            double miny = bitmap.PixelHeight + 2;
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

            //长边
            var AB = Math.Sqrt((p4s[0].X - p4s[1].X) * (p4s[0].X - p4s[1].X) + (p4s[0].Y - p4s[1].Y) * (p4s[0].Y - p4s[1].Y));
            var CD = Math.Sqrt((p4s[3].X - p4s[2].X) * (p4s[3].X - p4s[2].X) + (p4s[3].Y - p4s[2].Y) * (p4s[3].Y - p4s[2].Y));
            //短边
            var AC = Math.Sqrt((p4s[0].X - p4s[3].X) * (p4s[0].X - p4s[3].X) + (p4s[0].Y - p4s[3].Y) * (p4s[0].Y - p4s[3].Y));
            var BD = Math.Sqrt((p4s[1].X - p4s[2].X) * (p4s[1].X - p4s[2].X) + (p4s[1].Y - p4s[2].Y) * (p4s[1].Y - p4s[2].Y));

            var imgw = 440;
            var imgh = 140;
            var imgbytes = new byte[imgw * imgh * 4];
            ImageMatRGB img = new ImageMatRGB(imgbytes, imgw, imgh, ImageMatRGB.PixelBytesFormat.BGRA);
            for (int j = 0; j < imgh; j++)
            {
                for (int i = 0; i < imgw; i++)
                {
                    var aimx = (int)((AC + BD) * i / (imgw * 2) + p4s[0].X);
                    var aimy = (int)((AB + CD) * j / (imgh * 2) + p4s[0].Y);
                    img[j, i] = imageRGB[aimy, aimx];
                }
            }
            ShowImage(img);

        }
        public System.Windows.Point ret2linesP(HoughTransform.Line a, HoughTransform.Line b)
        {
            var P = new System.Windows.Point();

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
        public bool isokp(System.Windows.Point p, BitmapImage bmp)
        {
            double width = bmp.PixelWidth;
            double height = bmp.PixelHeight;

            if (p.X < 0 || p.X > width)
                return false;

            if (p.Y < 0 || p.Y > height)
                return false;

            return true;
        }
        public bool isinside(System.Windows.Point A, System.Windows.Point B, System.Windows.Point C, System.Windows.Point D, System.Windows.Point P)
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


        public static byte[] BitmapImageToByteArray(ref BitmapImage bmp)
        {
            byte[] byteArray = null;
            try
            {
                Stream sMarket = bmp.StreamSource;
                if (sMarket != null && sMarket.Length > 0)
                {
                    //很重要，因为Position经常位于Stream的末尾，导致下面读取到的长度为0。 
                    sMarket.Position = 0;

                    using (BinaryReader br = new BinaryReader(sMarket))
                    {
                        byteArray = br.ReadBytes((int)sMarket.Length);
                    }
                }
            }
            catch (Exception e)
            {
                //other exception handling 
            }
            return byteArray;
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {



            var pos = e.GetPosition(image);
            var pixelRGB = imageRGB[(int)pos.Y][(int)pos.X];
            var pixelHSV = imageHSV[(int)pos.Y][(int)pos.X];

            //MessageBox.Show($"R:{pixelRGB.R},G:{pixelRGB.G},B:{pixelRGB.B}\nH:{pixelHSV.H},S:{pixelHSV.S},V:{pixelHSV.V}");
            MessageBox.Show($"x:{pos.X},y:{pos.Y}");
        }

    }
}
