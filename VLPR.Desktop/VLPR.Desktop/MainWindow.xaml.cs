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
            bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\沪KR9888.png"));
            //bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\苏B79999.jpg"));
            //bitmap = new BitmapImage(new Uri(@"D:\Users\zzq\Desktop\PIC\沪KR9888.png"));
            //bitmap = new BitmapImage(new Uri(@"D:\Users\zzq\Desktop\PIC\京N8P8F8.jpg"));
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
            //ShowImage(showImage);
            //image.Source = showImage.ToWriteableBitmap();
            //var k = bwImage.ToImageMatRGB();
            //image_HoughTransform(imageBW, k);
            //image.Source = k.ToWriteableBitmap();
            var img = InversePerspectiveHelper<ImageMatRGB,PixelRGB>.InversePerspective(imageRGB, lines); 
            ShowImage(img);

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
