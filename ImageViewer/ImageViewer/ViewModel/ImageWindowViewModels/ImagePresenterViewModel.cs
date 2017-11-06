using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Model;
using Prism.Events;
using ImageViewer.Model.Event;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageViewer.Methods;
using ImageViewer.View;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ImagePresenterViewModel : BaseViewModel
    {
        private int _mouseX;
        private int _mouseY;
        private Image _displayedImage;
        private BitmapSource _imageSource;
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageClickCommand { get; set; }
        private static ITool tool = null;
        public int ImageWidth;
        public int ImageHeight;
        public static ITool Tool
        {
            get
            {
                return tool;
            }
            set
            {
                tool = value;
            }
        }
        public Image DisplayedImage
        {
            get
            {
                return _displayedImage;
            }
            set
            {
                _displayedImage = value;
                NotifyPropertyChanged();
                if(_displayedImage != null)
                    ImageSource = new BitmapImage(new Uri(_displayedImage.FilePath));
            }
        }
        public BitmapSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            { 
                _imageSource = value;
                NotifyPropertyChanged();
            }
        }

        public int MouseX { get{ return _mouseX; }
            set
            {
                if (_mouseX == value)
                    return;
                _mouseX = value;
                NotifyPropertyChanged();
            }
        }

        public int MouseY
        {
            get { return _mouseY; }
            set
            {
                if (_mouseY == value)
                    return;
                _mouseY = value;
                NotifyPropertyChanged();
            }
        }

        public ImagePresenterViewModel()
        {
            _aggregator.GetEvent<DisplayImage>().Subscribe(item => { DisplayedImage = item; });
            //ImageClickCommand = new RelayCommand(ImageClickExecute);
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageClickExecute);
        }

        private void ImageClickExecute(System.Windows.RoutedEventArgs args)
        {
            object obj = new object();
            if(tool != null)
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tool.AffectImage(ImageSource, obj, _mouseX, _mouseY);
                }));
        }
    }
}
