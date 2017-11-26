using ImageViewer.Model.Event;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    class CreateRegion : ITool
    {
        public void AffectImage(Dictionary<String, Object> args)
        {
            try
            {
                BitmapSource bitmapSource = (BitmapSource)args["BitmapSource"];
                System.Windows.Point regionLocation = (System.Windows.Point)args["BoundingBoxLocation"];
                int regionWidth = (int)args["BoundingBoxWidth"];
                int regionHeight = (int)args["BoundingBoxHeight"];
                if (regionWidth <= 0 || regionHeight <= 0)
                    return;
                int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
                int size = bitmapSource.PixelHeight * stride;
                byte[] pixels = new byte[size];

                bitmapSource.CopyPixels(pixels, stride, 0);
                Bitmap bitmap;
                //using (MemoryStream outStream = new MemoryStream())
                    //{
                    //    BitmapEncoder enc = new BmpBitmapEncoder();

                    //    enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                    //    enc.Save(outStream);
                    //    bitmap = new System.Drawing.Bitmap(outStream);
                    //    bitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\bitmap2.png", ImageFormat.Png);
                    //}
                bitmap = GetBitmap(bitmapSource);
                bitmap = getGrayOverlayLBA(bitmap, (int)regionLocation.X, (int)regionLocation.Y, regionWidth, regionHeight);
                //using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                //{
                //    double pixelWidth = (graphics.DpiX / bitmapSource.DpiX);
                //    double pixelHeight = (graphics.DpiY / bitmapSource.DpiY);
                //    bitmap = CopyBitmap(bitmap, new Rectangle((int)(regionLocation.X * bitmapSource.DpiX/96.0), (int)(regionLocation.Y * bitmapSource.DpiY / 96.0), (int)(regionWidth*pixelWidth * bitmapSource.DpiX/96.0), (int)(regionHeight*pixelHeight * bitmapSource.DpiX / 96.0)));
                //}
                bitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\bitmap.png", ImageFormat.Png);
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

                Dictionary<string,Object> regionInformation = new Dictionary<string, Object>();
                regionInformation.Add("Averages", averages);
                regionInformation.Add("Mins", mins);
                regionInformation.Add("Maxs", maxs);

                IEventAggregator _aggregator = GlobalEvent.GetEventAggregator();
                _aggregator.GetEvent<SendRegionInformationEvent>().Publish(regionInformation);

            }
            catch(KeyNotFoundException)
            {

            }
        }
        Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
        public Bitmap getGrayOverlayLBA(Bitmap bmp1, int posX, int posY, int width, int height)
        {
            System.Drawing.Size s1 = bmp1.Size;
            System.Drawing.Imaging.PixelFormat fmt1 = bmp1.PixelFormat;

            System.Drawing.Imaging.PixelFormat fmt = new System.Drawing.Imaging.PixelFormat();
            fmt = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            Bitmap bmp3 = new Bitmap(s1.Width, s1.Height, fmt);

            Rectangle rect = new Rectangle(posX, posY, width, height);

            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, fmt1);
            BitmapData bmp3Data = bmp3.LockBits(rect, ImageLockMode.ReadWrite, fmt);

            byte bpp1 = 4;
            byte bpp3 = 4;

            if (fmt1 == System.Drawing.Imaging.PixelFormat.Format24bppRgb) bpp1 = 3;
            else if (fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppArgb) bpp1 = 4; else return null;

            int size1 = bmp1Data.Stride * bmp1Data.Height;
            int size3 = bmp3Data.Stride * bmp3Data.Height;
            byte[] data1 = new byte[size1];
            byte[] data3 = new byte[size3];
            System.Runtime.InteropServices.Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
            System.Runtime.InteropServices.Marshal.Copy(bmp3Data.Scan0, data3, 0, size3);

            for (int y = 0; y < s1.Height; y++)
            {
                for (int x = 0; x < s1.Width; x++)
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
                    data3[index3 + 3] = A;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(data3, 0, bmp3Data.Scan0, data3.Length);
            bmp1.UnlockBits(bmp1Data);
            bmp3.UnlockBits(bmp3Data);
            return bmp3;
        }
        public BitmapSource Convert(System.Drawing.Bitmap bitmap)
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

        public static byte[] BitmapToByteArray(Bitmap bitmap)
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

        protected Bitmap CopyBitmap(Bitmap source, Rectangle part)
        {
            Bitmap bmp = new Bitmap(part.Width, part.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(source, 0, 0, part, GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }

        public Tools GetToolEnum()
        {
            return Tools.RegionSelection;
        }
    }
}
