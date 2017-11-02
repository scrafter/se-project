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

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ImagePresenterViewModel : BaseViewModel
    {
        private Image _displayedImage;
        private ImageSource _imageSource;

        public Image DisplayedImage
        {
            get => _displayedImage;
            set
            {
                _displayedImage = value;
                NotifyPropertyChanged();
                if(_displayedImage != null)
                    ImageSource = new BitmapImage(new Uri(_displayedImage.FilePath));
            }
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            { 
                _imageSource = value;
                NotifyPropertyChanged();
            }
        }

        public ImagePresenterViewModel()
        {
            _aggregator.GetEvent<DisplayImage>().Subscribe(item => { DisplayedImage = item; });
        }
    }
}
