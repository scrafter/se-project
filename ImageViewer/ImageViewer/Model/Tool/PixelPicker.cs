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
        public void AffectImage(Dictionary<String, Object> args)
        {
            try
            {
                BitmapSource bitmapSource = (BitmapSource)args["BitmapSource"];
                int mouseX = (int)((int)args["MouseX"] * bitmapSource.DpiX / 96);
                int mouseY = (int)((int)args["MouseY"] * bitmapSource.DpiY / 96);

                int stride = bitmapSource.PixelWidth * 4;
                int size = bitmapSource.PixelHeight * stride;
                byte[] pixels = new byte[size];
                bitmapSource.CopyPixels(pixels, stride, 0);
                int index = mouseY * stride + 4 * mouseX;

                byte red = pixels[index + 2];
                byte green = pixels[index + 1];
                byte blue = pixels[index];
                byte alpha = pixels[index + 3];

                Dictionary<string, int> pixelInformation = new Dictionary<string, int>();
                pixelInformation.Add("Alpha", alpha);
                pixelInformation.Add("Red", red);
                pixelInformation.Add("Green", green);
                pixelInformation.Add("Blue", blue);
                pixelInformation.Add("MouseX", mouseX);
                pixelInformation.Add("MouseY", mouseY);

                IEventAggregator aggregator = GlobalEvent.GetEventAggregator();
                aggregator.GetEvent<SendPixelInformationEvent>().Publish(pixelInformation);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
        }

        public Tools GetToolEnum()
        {
            return Tools.PixelInformations;
        }
    }
}
