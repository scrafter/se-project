using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageViewer.Methods
{
    class BitmapWorker
    {
        public System.Drawing.Bitmap GetBitmap(BitmapSource src)
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
        public Bitmap GetBitmapFragment(Bitmap bmp1, int posX, int posY, int width, int height)
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
                    System.Drawing.Color c1;

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
        public byte[] BitmapToByteArray(Bitmap bitmap)
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
        public BitmapImage ByteArrayToBitmapImage(ref byte[] array)
        {
            try
            {
                // PropertyChanged method
                BitmapImage bmpi = new BitmapImage();
                bmpi.BeginInit();
                bmpi.StreamSource = new MemoryStream(array);
                bmpi.EndInit();
                return bmpi;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public Bitmap Superimpose(Bitmap largeBmp, Bitmap smallBmp, int x, int y)
        {
            Graphics g = Graphics.FromImage(largeBmp);
            smallBmp.MakeTransparent();
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(smallBmp, new System.Drawing.Point(x, y));
            return largeBmp;
        }

        public Bitmap ClearBitmap(Bitmap bmp)
        {
            Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
            using (var g = Graphics.FromImage(bitmap))
            { 
                g.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            return bitmap;
        }
        public BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
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
            catch (Exception e)
            {

                return null;
            }
        }
    }
}
