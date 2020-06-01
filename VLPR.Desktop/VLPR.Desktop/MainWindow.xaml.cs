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
        BitmapImage bitmap;


        SortedDictionary<byte, int> Hs = new SortedDictionary<byte, int>();
        SortedDictionary<double, int> Ss = new SortedDictionary<double, int>();
        SortedDictionary<byte, int> Vs = new SortedDictionary<byte, int>();


        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\沪KR9888.png"));
            //bitmap = new BitmapImage(new Uri(@"D:\Pictures\VLPR\苏B79999.jpg"));

            //image.Source = bitmap;



            var bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            var stride = bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8;
            bitmap.CopyPixels(bytes, stride, 0);

            imageRGB = new ImageMatRGB(bytes, bitmap.PixelWidth, bitmap.PixelHeight, ImageMatRGB.PixelBytesFormat.BGRA);
            imageHSV = imageRGB.ToImageHSV();

            //image.Source = imageRGB.ToWriteableBitmap();

            int range = 2;
            var bwImage = imageHSV.ToImageBlackWhite((x, y, p) => p.H >= 200 && p.S >= 0.90 && p.V >= 100).Dilation(range).Erode(range);
            //var raw_regions = bwImage.SearchConnectedRegion(8);
            //var erase_regions = raw_regions.Where(kv => kv.Value.Count < 1000).Select(kv => kv.Value);
            //var save_regions = raw_regions.Where(kv => kv.Value.Count > 1000).Select(kv => kv.Value);
            //foreach (var region in erase_regions)
            //{
            //    foreach (var p in region)
            //    {
            //        bwImage[p.y][p.x] = false;
            //    }
            //}
                //bwImage = bwImage.ToImageBlackWhite((x, y, p) =>
                //{
                //    if (!p)
                //        return false;

                //    bool flag = false;


                //    for (int i = -1; i <= 1; i++)
                //    {
                //        for (int j = -1; j <= 1; j++)
                //        {
                //            if (!bwImage[y + j][x + i])
                //            {
                //                flag = true;
                //                break;
                //            }
                //        }
                //    }
                //    return flag;
                //});

                image.Source = bwImage.ToWriteableBitmap();


            image.Width = bitmap.PixelWidth;
            image.Height = bitmap.PixelHeight;
        }

        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
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
            catch(Exception e)
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

            MessageBox.Show($"R:{pixelRGB.R},G:{pixelRGB.G},B:{pixelRGB.B}\nH:{pixelHSV.H},S:{pixelHSV.S},V:{pixelHSV.V}");
        }
    }
}
