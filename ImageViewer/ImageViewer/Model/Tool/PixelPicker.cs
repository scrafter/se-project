using ImageViewer.Model.Event;
using Prism.Events;
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
        public void AffectImage(BitmapSource bitmapSource, object obj, int mouseX, int mouseY)
        {
            int stride = bitmapSource.PixelWidth * 4;
            int size = bitmapSource.PixelHeight * stride;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);
            int index = mouseY * stride + 4 * mouseX;

            byte red = pixels[index];
            byte green = pixels[index + 1];
            byte blue = pixels[index + 2];
            byte alpha = pixels[index + 3];
            Dictionary<string, byte> RGBA = new Dictionary<string, byte>();
            RGBA.Add("Red", red);
            RGBA.Add("Green", green);
            RGBA.Add("Blue", blue);
            RGBA.Add("Alpha", alpha);
            IEventAggregator _aggregator = GlobalEvent.GetEventAggregator();
            _aggregator.GetEvent<SendPixelInformationEvent>().Publish(RGBA);
        }
    }
}
