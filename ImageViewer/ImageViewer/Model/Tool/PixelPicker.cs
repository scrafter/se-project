using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    class PixelPicker : ITool
    {
        public PixelPicker()
        {

        }
        public void AffectImage(BitmapSource bitmapSource, object obj)
        {
            int stride = bitmapSource.PixelWidth * 4;
            int size = bitmapSource.PixelHeight * stride;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);
        }
    }
}
