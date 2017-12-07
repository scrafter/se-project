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
        public Bitmap GetBitmapFragment(Bitmap bmp1, int posX, int posY, int width, int height, int offsetX, int offsetY)
        {
            System.Drawing.Size s1 = bmp1.Size;
            PixelFormat fmt1 = bmp1.PixelFormat;

            PixelFormat fmt = PixelFormat.Format32bppArgb;
            Bitmap bmp2 = new Bitmap(width, height, fmt);
            BitmapData bmp2Data = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, fmt);
            posX = posX - offsetX;
            posY = posY - offsetY;

            if (posX > bmp1.Width || posY > bmp1.Height)
                return bmp2;
            
            
            if (posX < 0)
            {
                width = width - Math.Abs(posX);
            }
            if (posY < 0)
            {
                height = height - Math.Abs(posY);
            }
            if(posX + width > bmp1.Width)
            {
                width = bmp1.Width - posX;
            }
            if(posY + height > bmp1.Height)
            {
                height = bmp1.Height - posY;
            }
            Rectangle rect = new Rectangle(posX < 0 ? 0 : posX, posY < 0 ? 0 : posY, width, height);

            BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, fmt1);
            

            byte bpp1 = 4;
            byte bpp3 = 4;

            if (fmt1 == PixelFormat.Format24bppRgb) bpp1 = 3;
            else if (fmt1 == PixelFormat.Format32bppPArgb
                    || fmt1 == PixelFormat.Format32bppArgb
                    || fmt1 == PixelFormat.Format32bppRgb)
                bpp1 = 4;
            else return bmp2;

            //int size1 = bmp1Data.Stride * bmp1Data.Height;
            int size1 = (height-1 )* bmp1Data.Stride + width*bpp1;
            int size = bmp1Data.Stride * bmp1Data.Height;
            int size2 = bmp2Data.Stride * bmp2Data.Height;
            byte[] data1 = new byte[size1];
            byte[] data2 = new byte[size2];
            try
            {
                Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
                Marshal.Copy(bmp2Data.Scan0, data2, 0, size2);
            }
            catch (AccessViolationException e)
            {
                return bmp2;
            }

            for (int y = 0; y < bmp2Data.Height; y++)
            {
                for (int x = 0; x < bmp2Data.Width; x++)
                {
                    int row = y + (posY > 0 ? 0 : posY);
                    int column = x + (posX > 0 ? 0 : posX);
                    int index3 = y * bmp2Data.Stride + x * bpp3;
                    if (row >= height || row < 0 || column >= width || column < 0)
                    {
                        continue;
                    }
                    else
                    {
                        int index1 = row * bmp1Data.Stride + column * bpp1;
                        System.Drawing.Color c1;

                        if (bpp1 == 4)
                            c1 = Color.FromArgb(data1[index1 + 3], data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);
                        else c1 = Color.FromArgb(255, data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);

                        data2[index3 + 0] = c1.B;
                        data2[index3 + 1] = c1.G;
                        data2[index3 + 2] = c1.R;
                        data2[index3 + 3] = c1.A;
                    }
                }
            }

            Marshal.Copy(data2, 0, bmp2Data.Scan0, data2.Length);
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            return bmp2;
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
                    System.Windows.Media.PixelFormats.Bgr32, null,
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
