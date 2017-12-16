using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageViewer.Model.Event
{
    public class ZoomEvent : PubSubEvent<ZoomEvent>
    {
        public double Zoom { get; set; }

        public ZoomEvent()
        {

        }
        public ZoomEvent(double zoom)
        {
            Zoom = zoom;
        }
    }
}
