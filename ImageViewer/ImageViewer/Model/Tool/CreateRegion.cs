using ImageViewer.Model.Event;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ImageViewer.Model
{
    class CreateRegion : ITool
    {
        public void AffectImage(Dictionary<String, Object> args)
        {
            try
            {
                BitmapSource bitmapSource = (BitmapSource)args["BitmapSource"];
                System.Windows.Point regionLocation = (System.Windows.Point)args["RegionLocation"];
                int regionWidth = (int)args["RegionWidth"];
                int regionHeight = (int)args["RegionHeight"];
                if (regionWidth <= 0 || regionHeight <= 0)
                    return;
                int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
                int size = bitmapSource.PixelHeight * stride;
                byte[] pixels = new byte[size];

                bitmapSource.CopyPixels(pixels, stride, 0);
                Bitmap bitmap;
                using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    bitmap = GetBitmap(bitmapSource);
                    bitmap = GetBitmapFragment(bitmap, (int)(regionLocation.X * bitmapSource.DpiX / 96.0), (int)(regionLocation.Y * bitmapSource.DpiY / 96.0), (int)(regionWidth * bitmapSource.DpiX / 96.0), (int)(regionHeight * bitmapSource.DpiY / 96.0));
                }


                bitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png", ImageFormat.Png);
                bitmapSource = Convert(bitmap);
                stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
                size = regionHeight * stride;
                pixels = BitmapToByteArray(bitmap); ;
                bitmapSource.CopyPixels(pixels, stride, 0);

                List<byte> alphas = new List<byte>();
                List<byte> reds = new List<byte>();
                List<byte> greens = new List<byte>();
                List<byte> blues = new List<byte>();

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    alphas.Add(pixels[i + 3]);
                    reds.Add(pixels[i + 2]);
                    greens.Add(pixels[i + 1]);
                    blues.Add(pixels[i]);
                }
                double[] averages = new double[4];
                averages[0] = 0;
                averages[1] = 0;
                averages[2] = 0;
                averages[3] = 0;
                for (int i = 0; i < alphas.Count; i++)
                {
                    averages[0] += reds[i];
                    averages[1] += greens[i];
                    averages[2] += blues[i];
                    averages[3] += alphas[i];
                }
                averages[0] /= reds.Count;
                averages[1] /= greens.Count;
                averages[2] /= blues.Count;
                averages[3] /= alphas.Count;

                byte[] mins = new byte[4];
                mins[0] = reds.Min();
                mins[1] = greens.Min();
                mins[2] = blues.Min();
                mins[3] = alphas.Min();

                byte[] maxs = new byte[4];
                maxs[0] = reds.Max();
                maxs[1] = greens.Max();
                maxs[2] = blues.Max();
                maxs[3] = alphas.Max();

                double[] variances = new double[4];
                double[] deviations = new double[4];

                GetVarianceAndDeviation(ref variances, ref deviations, averages, reds, greens, blues, alphas);

                Dictionary<string,Object> regionInformation = new Dictionary<string, Object>();
                regionInformation.Add("Averages", averages);
                regionInformation.Add("Mins", mins);
                regionInformation.Add("Maxs", maxs);
                regionInformation.Add("Width", bitmap.Width);
                regionInformation.Add("Height", bitmap.Height);
                regionInformation.Add("Variances", variances);
                regionInformation.Add("Deviations", deviations);

                IEventAggregator _aggregator = GlobalEvent.GetEventAggregator();
                _aggregator.GetEvent<SendRegionInformationEvent>().Publish(regionInformation);

            }
            catch(KeyNotFoundException)
            {

            }
        }
        public static System.Drawing.Bitmap GetBitmap(BitmapSource src)
        {
            try
            {
                MemoryStream TransportStream = new MemoryStream();
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(src));
                enc.Save(TransportStream);
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(TransportStream);
                TransportStream.Close();
                TransportStream.Dispose();
                return bmp;
            }
            catch { MessageBox.Show("failed"); return null; }
        }
        private void GetVarianceAndDeviation(ref double[] variances, ref double[] deviations, double[] averages, List<byte> reds, List<byte> greens, List<byte> blues, List<byte> alphas)
        {
            double sum;
            //Red
            sum = 0.0;
            for (int i = 0; i < reds.Count; i++)
            {
                sum += Math.Pow(reds[i] - averages[0], 2);
            }
            variances[0] = sum / reds.Count;
            deviations[0] = Math.Sqrt(variances[0]);
            //Green
            sum = 0.0;
            for (int i = 0; i < greens.Count; i++)
            {
                sum += Math.Pow(greens[i] - averages[1], 2);
            }
            variances[1] = sum / greens.Count;
            deviations[1] = Math.Sqrt(variances[1]);
            //Blue
            sum = 0.0;
            for (int i = 0; i < blues.Count; i++)
            {
                sum += Math.Pow(blues[i] - averages[2], 2);
            }
            variances[2] = sum / blues.Count;
            deviations[2] = Math.Sqrt(variances[2]);
            //Alpha
            sum = 0.0;
            for (int i = 0; i < alphas.Count; i++)
            {
                sum += Math.Pow(alphas[i] - averages[3], 2);
            }
            variances[3] = sum / alphas.Count;
            deviations[3] = Math.Sqrt(variances[3]);

        }
        public static Bitmap GetBitmapFragment(Bitmap bmp1, int posX, int posY, int width, int height)
        {
            System.Drawing.Size s1 = bmp1.Size;
            System.Drawing.Imaging.PixelFormat fmt1 = bmp1.PixelFormat;

            System.Drawing.Imaging.PixelFormat fmt = new System.Drawing.Imaging.PixelFormat();
            fmt = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            Bitmap bmp3 = new Bitmap(width, height, fmt);

            Rectangle rect = new Rectangle(posX, posY, width, height);

            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, fmt1);
            BitmapData bmp3Data = bmp3.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, fmt);

            byte bpp1 = 4;
            byte bpp3 = 4;

            if (fmt1 == System.Drawing.Imaging.PixelFormat.Format24bppRgb) bpp1 = 3;
            else if (fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppPArgb || fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppArgb || fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppRgb) bpp1 = 4; else return null;

            int size1 = bmp1Data.Stride * bmp1Data.Height;
            int size3 = bmp3Data.Stride * bmp3Data.Height;
            byte[] data1 = new byte[size1];
            byte[] data3 = new byte[size3];
            System.Runtime.InteropServices.Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
            System.Runtime.InteropServices.Marshal.Copy(bmp3Data.Scan0, data3, 0, size3);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index1 = y * bmp1Data.Stride + x * bpp1;
                    int index3 = y * bmp3Data.Stride + x * bpp3;
                    System.Drawing.Color c1, c2;

                    if (bpp1 == 4)
                        c1 = System.Drawing.Color.FromArgb(data1[index1 + 3], data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);
                    else c1 = System.Drawing.Color.FromArgb(255, data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);

                    byte A = (byte)(255 * c1.GetBrightness());
                    data3[index3 + 0] = c1.B;
                    data3[index3 + 1] = c1.G;
                    data3[index3 + 2] = c1.R;
                    data3[index3 + 3] = c1.A;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(data3, 0, bmp3Data.Scan0, data3.Length);
            bmp1.UnlockBits(bmp1Data);
            bmp3.UnlockBits(bmp3Data);
            return bmp3;
        }
        private BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            try
            {
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr32, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
                return bitmapSource;
            }
            catch(Exception e)
            {

                return null;
            }
        }

        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {

            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }

        public Tools GetToolEnum()
        {
            return Tools.RegionSelection;
        }
    }
}
