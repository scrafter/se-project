using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                int stride = regionWidth * 4;
                int size = regionHeight * stride;
                byte[] pixels = new byte[size];

                //bitmapSource.CopyPixels(rowPixels, stride, 0);
                Bitmap bitmap;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();

                    enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                    enc.Save(outStream);
                    bitmap = new System.Drawing.Bitmap(outStream);
                }
                using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    double pixelWidth = (graphics.DpiX / bitmapSource.DpiX);
                    double pixelHeight = (graphics.DpiY / bitmapSource.DpiY);
                    bitmap = CopyBitmap(bitmap, new Rectangle((int)(regionLocation.X * bitmapSource.DpiX/96.0), (int)(regionLocation.Y * bitmapSource.DpiY / 96.0), (int)(regionWidth*pixelWidth), (int)(regionHeight*pixelHeight)));
                }
                bitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ImageViewer\bitmap.bmp", ImageFormat.Bmp);
                //List<byte> alphas = new List<byte>();
                //List<byte> reds = new List<byte>();
                //List<byte> greens = new List<byte>();
                //List<byte> blues = new List<byte>();

                //for (int i = 0; i < pixels.Length; i+=4)
                //{
                //    alphas.Add(pixels[i + 3]);
                //    reds.Add(pixels[i + 2]);
                //    greens.Add(pixels[i + 1]);
                //    blues.Add(pixels[i]);
                //}
            }
            catch(KeyNotFoundException)
            {

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
