using ImageViewer.Model.Event;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace ImageViewer.Model
{
    class Rotate : ITool
    {

        public Rotate()
        {


        }
        public void AffectImage(Dictionary<String, Object> args)
        {
            Image image = (Image)args["DisplayedImage"];
            int presenterID = (int)args["PresenterID"];
            var transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = image.Bitmap;
            transformedBitmap.Transform = new RotateTransform(90);
            transformedBitmap.EndInit();
            image.Bitmap = transformedBitmap;

            SendDisplayedImage sdi = new SendDisplayedImage();
            IEventAggregator aggregator = GlobalEvent.GetEventAggregator();
            sdi.Image = image;
            sdi.PresenterID = presenterID;
            aggregator.GetEvent<SendDisplayedImage>().Publish(sdi);

        }

        public Tools GetToolEnum()
        {
            return Tools.Rotate;
        }
    }
}
