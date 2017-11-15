using ImageViewer.View;
using ImageViewer.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class PixelInformationViewModel : BaseViewModel
    {
        private Brush _pixelColor;
        private String _rgbaValue;
        private int _mouseX;
        private int _mouseY;
        public Brush PixelColor
        {
            get
            {
                return _pixelColor;
            }
            set
            {
                _pixelColor = value;
                NotifyPropertyChanged();
            }
        }
        public String RGBAValue
        {
            get
            {
                return _rgbaValue;
            }
            set
            {
                _rgbaValue = value;
                NotifyPropertyChanged();
            }
        }

        public int MouseX {
            get
            {
                return _mouseX;
            }
            set
            {
                _mouseX = value;
            }
        }
        public int MouseY
        {
            get
            {
                return _mouseY;
            }
            set
            {
                _mouseY = value;
            }
        }

        public PixelInformationViewModel()
        {
            _aggregator.GetEvent<SendPixelInformationEvent>().Subscribe(SetPixelInformations);
        }

        private void SetPixelInformations(Dictionary<String, byte> RGBA)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                byte[] rgba = { RGBA["Alpha"], RGBA["Red"], RGBA["Green"], RGBA["Blue"] };
                Color c = Color.FromArgb(RGBA["Alpha"], RGBA["Red"], RGBA["Green"], RGBA["Blue"]);
                this.PixelColor = new SolidColorBrush(c);
                StringBuilder sb = new StringBuilder();
                sb.Append("Red: ");
                sb.AppendLine(c.R.ToString());
                sb.Append("Green: ");
                sb.AppendLine(c.G.ToString());
                sb.Append("Blue: ");
                sb.AppendLine(c.B.ToString());
                sb.Append("Alpha: ");
                sb.Append(c.A.ToString());
                RGBAValue = sb.ToString();
            }));
        }
    }
}
