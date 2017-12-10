using ImageViewer.Methods;
using ImageViewer.Model.Event;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    class MagnifyingGlass : ITool
    {
        int zoomPositionXBeg;
        int zoomPositionYBeg;


        int zoomPositionXEnd;
        int zoomPositionYEnd;

        public MagnifyingGlass()
        {
           
        }
        public void AffectImage(Dictionary<String, Object> args)
        {

            try
            {
                IEventAggregator aggregator;
                ZoomingInfo zi;
                Image image = (Image)args["DisplayedImage"];
                BitmapSource bitmapSource = image.OriginalBitmap;
                int clickPositionX = (int)args["ClickPositionX"];
                int clickPositionY = (int)args["ClickPositionY"];
                int imageWidth = (int)(double)args["ImageWidth"];
                int imageHeight = (int)(double)args["ImageHeight"];
                double zoomValue = (double)args["ZoomValue"];

                if (zoomValue >= 1)
                {
                    aggregator = GlobalEvent.GetEventAggregator();
                    zi = new ZoomingInfo();
                    image.Bitmap = image.OriginalBitmap;
                    zi.ImageToBeDisplayed = image;
                    aggregator.GetEvent<SendZoomEvent>().Publish(zi);
                }
                else {
                    zoomPositionXBeg = clickPositionX - (int)(0.5 * (zoomValue * imageWidth));
                    zoomPositionYBeg = clickPositionY - (int)(0.5 * (zoomValue * imageHeight));

                    zoomPositionXEnd = clickPositionX + (int)(0.5 * (zoomValue * imageWidth));
                    zoomPositionYEnd = clickPositionY + (int)(0.5 * (zoomValue * imageHeight));

                    AdjustZoomBordersIfNecessary(imageWidth, imageHeight);


                    Bitmap bitmap;

                    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                    {
                        bitmap = GetBitmap(bitmapSource);
                        bitmap = GetBitmapFragment(bitmap, (int)(zoomPositionXBeg * bitmapSource.DpiX / 96.0), (int)(zoomPositionYBeg * bitmapSource.DpiY / 96.0), (int)(zoomPositionXEnd * bitmapSource.DpiX / 96.0), (int)(zoomPositionYEnd * bitmapSource.DpiY / 96.0));

                    }

                    Bitmap finalBitmap = new Bitmap(bitmap, new System.Drawing.Size((int)bitmapSource.Width, (int)bitmapSource.Height));

                    BitmapWorker bw = new BitmapWorker();

                    bitmapSource = bw.BitmapToSource(finalBitmap);

                    image.Bitmap = bitmapSource;

                    //finalBitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png", ImageFormat.Png);

                    aggregator = GlobalEvent.GetEventAggregator();
                    zi = new ZoomingInfo();
                    zi.ImageToBeDisplayed = image;
                    aggregator.GetEvent<SendZoomEvent>().Publish(zi);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        private void AdjustZoomBordersIfNecessary(int imageWidth, int imageHeight)
        {
            if (zoomPositionXBeg < 0)
            {
                int offsetX = 0 - zoomPositionXBeg;
                zoomPositionXBeg += offsetX;
                zoomPositionXEnd += offsetX;
            }

            if (zoomPositionYBeg < 0)
            {
                int offsetY = 0 - zoomPositionYBeg;
                zoomPositionYBeg += offsetY;
                zoomPositionYEnd += offsetY;
            }

            if (zoomPositionXEnd > imageWidth)
            {
                int offsetX = zoomPositionXEnd - imageWidth;
                zoomPositionXBeg -= offsetX;
                zoomPositionXEnd -= offsetX;
            }

            if (zoomPositionYEnd > imageHeight)
            {
                int offsetY = zoomPositionYEnd - imageHeight;
                zoomPositionYBeg -= offsetY;
                zoomPositionYEnd -= offsetY;
            }

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

        public Tools GetToolEnum()
        {
            return Tools.Magnifier;
        }
    }
}
