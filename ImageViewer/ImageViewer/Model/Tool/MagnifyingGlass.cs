using ImageViewer.Methods;
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
                //ZoomingInfo zi;
                Image image = (Image)args["DisplayedImage"];
                BitmapSource bitmapSource = image.OriginalBitmap;
                int clickPositionX = (int)((int)args["ClickPositionX"] * bitmapSource.DpiX / 96.0);
                int clickPositionY = (int)((int)args["ClickPositionY"] * bitmapSource.DpiY / 96.0);
                int imageWidth = bitmapSource.PixelWidth;
                int imageHeight = bitmapSource.PixelHeight;
                double zoomValue = (double)args["ZoomValue"];

                if (zoomValue >= 1)
                {
                    aggregator = GlobalEvent.GetEventAggregator();
                    //zi = new ZoomingInfo();
                    //image.Bitmap = image.OriginalBitmap;
                    //zi.ZoomScale = 1;
                    //zi.ImagePositionX = 0;
                    //zi.ImagePositionY = 0;
                    //aggregator.GetEvent<SendZoomEvent>().Publish(zi);
                }
                else
                {
                    zoomPositionXBeg = clickPositionX - (int)(0.5 * (zoomValue * imageWidth));
                    zoomPositionYBeg = clickPositionY - (int)(0.5 * (zoomValue * imageHeight));

                    zoomPositionXEnd = clickPositionX + (int)(0.5 * (zoomValue * imageWidth));
                    zoomPositionYEnd = clickPositionY + (int)(0.5 * (zoomValue * imageHeight));

                    AdjustZoomBordersIfNecessary(imageWidth, imageHeight);
                    int width = zoomPositionXEnd - zoomPositionXBeg;
                    int height = zoomPositionYEnd - zoomPositionYBeg;


                    Bitmap bitmap;

                    bitmap = GetBitmap(bitmapSource);
                    bitmap = GetBitmapFragment(bitmap, zoomPositionXBeg, zoomPositionYBeg, width, height, 0, 0);


                    Bitmap finalBitmap = new Bitmap(bitmap, new System.Drawing.Size( imageWidth, imageHeight));

                    BitmapWorker bw = new BitmapWorker();

                    bitmapSource = bw.BitmapToSource(finalBitmap);

                    image.Bitmap = bitmapSource;

                    //finalBitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\temp.png", ImageFormat.Png);

                    aggregator = GlobalEvent.GetEventAggregator();
                    //zi = new ZoomingInfo();
                    //zi.ZoomScale = 1/zoomValue;
                    //zi.ImagePositionX = zoomPositionXBeg;
                    //zi.ImagePositionY = zoomPositionYBeg;
                    //aggregator.GetEvent<SendZoomEvent>().Publish(zi);
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

            if (zoomPositionXEnd >= imageWidth)
            {
                int offsetX = zoomPositionXEnd - imageWidth;
                zoomPositionXBeg -= offsetX;
                zoomPositionXEnd -= offsetX;
            }

            if (zoomPositionYEnd >= imageHeight)
            {
                int offsetY = zoomPositionYEnd - imageHeight;
                zoomPositionYBeg -= offsetY;
                zoomPositionYEnd -= offsetY;
            }

        }

        //public static Bitmap GetBitmapFragment(Bitmap bmp1, int posX, int posY, int width, int height)
        //{
        //    System.Drawing.Size s1 = bmp1.Size;
        //    System.Drawing.Imaging.PixelFormat fmt1 = bmp1.PixelFormat;

        //    System.Drawing.Imaging.PixelFormat fmt = new System.Drawing.Imaging.PixelFormat();
        //    fmt = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        //    Bitmap bmp3 = new Bitmap(width, height, fmt);

        //    Rectangle rect = new Rectangle(posX, posY, width, height);

        //    BitmapData bmp1Data = bmp1.LockBits(rect, ImageLockMode.ReadOnly, fmt1);
        //    BitmapData bmp3Data = bmp3.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, fmt);

        //    byte bpp1 = 4;
        //    byte bpp3 = 4;

        //    if (fmt1 == System.Drawing.Imaging.PixelFormat.Format24bppRgb) bpp1 = 3;
        //    else if (fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppPArgb || fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppArgb || fmt1 == System.Drawing.Imaging.PixelFormat.Format32bppRgb) bpp1 = 4; else return null;

        //    int size1 = bmp1Data.Stride * bmp1Data.Height;
        //    int size3 = bmp3Data.Stride * bmp3Data.Height;
        //    byte[] data1 = new byte[size1];
        //    byte[] data3 = new byte[size3];
        //    System.Runtime.InteropServices.Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
        //    System.Runtime.InteropServices.Marshal.Copy(bmp3Data.Scan0, data3, 0, size3);

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int index1 = y * bmp1Data.Stride + x * bpp1;
        //            int index3 = y * bmp3Data.Stride + x * bpp3;
        //            System.Drawing.Color c1, c2;

        //            if (bpp1 == 4)
        //                c1 = System.Drawing.Color.FromArgb(data1[index1 + 3], data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);
        //            else c1 = System.Drawing.Color.FromArgb(255, data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);

        //            byte A = (byte)(255 * c1.GetBrightness());
        //            data3[index3 + 0] = c1.B;
        //            data3[index3 + 1] = c1.G;
        //            data3[index3 + 2] = c1.R;
        //            data3[index3 + 3] = c1.A;
        //        }
        //    }

        //    System.Runtime.InteropServices.Marshal.Copy(data3, 0, bmp3Data.Scan0, data3.Length);
        //    bmp1.UnlockBits(bmp1Data);
        //    bmp3.UnlockBits(bmp3Data);
        //    return bmp3;
        //}
        public Bitmap GetBitmapFragment(Bitmap bmp1, int posX, int posY, int width, int height, int offsetX, int offsetY)
        {
            System.Drawing.Size s1 = bmp1.Size;
            PixelFormat fmt1 = bmp1.PixelFormat;

            PixelFormat fmt = PixelFormat.Format32bppArgb;
            Bitmap bmp2 = new Bitmap(width, height, fmt);
            posX = posX - offsetX;
            posY = posY - offsetY;
            if (posX >= bmp1.Width || posY >= bmp1.Height || posX + width < 0 || posY + height < 0)
                return bmp2;
            BitmapData bmp2Data = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, fmt);
            if (posX < 0)
            {
                width = width - Math.Abs(posX);
            }
            if (posY < 0)
            {
                height = height - Math.Abs(posY);
            }
            if (posX + width > bmp1.Width)
            {
                width = bmp1.Width - posX;
            }
            if (posY + height > bmp1.Height)
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
            int size1 = (height - 1) * bmp1Data.Stride + width * bpp1;
            int size = bmp1Data.Stride * bmp1Data.Height;
            int size2 = bmp2Data.Stride * bmp2Data.Height;
            byte[] data1 = new byte[size1];
            byte[] data2 = new byte[size2];

            Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
            Marshal.Copy(bmp2Data.Scan0, data2, 0, size2);

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
