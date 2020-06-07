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
            lines.Sort((a, b) => (int)(a.Distance - b.Distance));


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

            MessageBox.Show($"R:{pixelRGB.R},G:{pixelRGB.G},B:{pixelRGB.B}\nH:{pixelHSV.H},S:{pixelHSV.S},V:{pixelHSV.V}");
        }

        //https://www.cnblogs.com/cheermyang/p/5348820.html
        //https://blog.csdn.net/jia20003/article/details/7724530
        private void image_HoughTransform(ImageMatBlackWhite inPixels, ImageMatRGB outPixels)
        {
            //var pixel = imageBW[1][0];
            //int[][] names = new int[5][];

            //霍夫空间,图像初始化
            int width = (int)inPixels.Width;
            int height = (int)inPixels.Height;

            int centerX = width / 2;
            int centerY = height / 2;

            int hough_space = 500;
            double hough_interval = Math.PI / (double)hough_space;

            int max = Math.Max(width, height);
            //r的最大值
            int max_length = (int)Math.Sqrt((2.0D) * max);

            int[][] hough_2d = new int[hough_space][];
            for (int k = 0; k < hough_space; k++)
            {
                hough_2d[k] = new int[2 * max_length];
            }

            //for (int i = 0; i < hough_space; i++)
            //{
            //    for (int j = 0; j < 2 * max_length; j++)
            //    {
            //        hough_2d[i][j] = 0;
            //    }
            //}



            int[][] image_2d = new int[height][];
            for (int k = 0; k < height; k++)
            {
                image_2d[k] = new int[width];
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    image_2d[i][j] = (inPixels[i][j] == true) ? 1 : 0;
                }
            }

            //从像素RGB空间到霍夫空间变换
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int p = image_2d[row][col] & 0xff;
                    if (p == 0) 
                        continue;


                    for (int cell = 0; cell < hough_space; cell++)
                    {
                        max = (int)((col - centerX) * Math.Cos(cell * hough_interval) + (row - centerY) * Math.Sin(cell * hough_interval));
                        max += max_length;
                        if (max < 0 || (max >= 2 * max_length))
                        {
                            continue;
                        }
                        hough_2d[cell][max] += 1;
                    }
                }
            }




            int[] hough_1d = new int[hough_space * 2 * max_length];
            for (int i = 0; i < hough_space; i++)
            {
                for (int j = 0; j < 2 * max_length; j++)
                {
                    hough_1d[i + j * hough_space] = hough_2d[i][j];
                }
            }

            //寻找最大霍夫值计算霍夫阈值
            var t = hough_1d.ToHashSet();
            var tt = t.Where(h => h > 10).ToList();
            tt.Sort(
                (x, y) =>
                {
                    if (x < y)
                        return 1;
                    else if (x == y)
                        return 0;
                    else
                        return -1;
                }
                );


            //int[] max_hough4 = new int[4];
            //for (int i = 0; i < 4; i++)
            //{
            //    max_hough4[i] = tt[i];
            //}

            //int max_hough = max_hough4[0];

            //float threshold = 0.5F;
            //int hough_threshold = (int)(threshold * max_hough);

            //从霍夫空间反变换回像素数据空间
            int hough_threshold = 80;


            for (int row = 0; row < hough_space; row++)
            {
                for (int col = 0; col < 2 * max_length; col++)
                {
                    if (hough_2d[row][col] < hough_threshold)
                        continue;

                    int hough_value = hough_2d[row][col];
                    bool isLine = true;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (i != 0 || j != 0)
                            {
                                int yf = row + i;
                                int xf = col + j;
                                if (xf < 0)
                                    continue;
                                if (xf < 2 * max_length)
                                {
                                    if (yf < 0)
                                    {
                                        yf += hough_space;
                                    }
                                    if (yf >= hough_space)
                                    {
                                        yf -= hough_space;
                                    }
                                    if (hough_2d[yf][xf] <= hough_value)
                                    {
                                        continue;
                                    }
                                    isLine = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (!isLine)
                        continue;

                    double dy = Math.Sin(row * hough_interval);
                    double dx = Math.Cos(row * hough_interval);
                    if ((row <= hough_space / 4) || (row >= 3 * hough_space / 4))
                    {
                        for (int subrow = 0; subrow < height; ++subrow)
                        {
                            int subcol = (int)((col - max_length - ((subrow - centerY) * dy)) / dx) + centerX;
                            if ((subcol < width) && (subcol >= 0))
                            {
                                image_2d[subrow][subcol] = -1;
                            }
                        }
                    }
                    else
                    {
                        for (int subcol = 0; subcol < width; ++subcol)
                        {
                            int subrow = (int)((col - max_length - ((subcol - centerX) * dx)) / dy) + centerY;
                            if ((subrow < height) && (subrow >= 0))
                            {
                                image_2d[subrow][subcol] = -1;
                            }
                        }
                    }
                }
            }


            /*for (int i = 0; i < hough_1d.Length; i++)
            {
                int value = hough_1d[i];
                if (value < 0)
                    value = 0;
                else if (value > 255)
                    value = 255;
                
                hough_1d[i] = (int)(0xFF000000 | value + (value << 16) + (value << 8));
            }*/

            

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (image_2d[row][col] == -1)
                    { 
                        outPixels[row][col].R = 255; 
                    }
                    else if (image_2d[row][col] == 0)
                    {
                        //outPixels[col][row].R = 0;
                        //outPixels[col][row].G = 0;
                        //outPixels[col][row].B = 0;
                        outPixels[row][col].R = 0;
                        outPixels[row][col].G = 0;
                        outPixels[row][col].B = 0;
                    }
                    else
                    {
                        outPixels[row][col].R = 255;
                        outPixels[row][col].G = 255;
                        outPixels[row][col].B = 255;
                    }
                }
            }


        }


    }
}
