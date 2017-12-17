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
            List<int> presenters = (List<int>)args["SynchronizedPresenters"];
            var transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = image.Bitmap;
            transformedBitmap.Transform = new RotateTransform(90);
            transformedBitmap.EndInit();
            image.Bitmap = transformedBitmap;

            RotateImageEvent ri = new RotateImageEvent();
            IEventAggregator aggregator = GlobalEvent.GetEventAggregator();
            presenters.Add(presenterID);
            ri.Image = image;
            ri.PresenterID = presenterID;
            ri.SynchronizedPresenters = presenters;
            aggregator.GetEvent<RotateImageEvent>().Publish(ri);

        }

        public Tools GetToolEnum()
        {
            return Tools.Rotate;
        }
    }
}
