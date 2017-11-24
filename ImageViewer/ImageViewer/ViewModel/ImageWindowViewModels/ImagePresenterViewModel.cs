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
using ImageViewer.View.ImagesWindow;
using System.Collections.ObjectModel;
using System.Windows;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ImagePresenterViewModel : BaseViewModel
    {
        private bool isDragged = false;
        private int _mouseX;
        private int _mouseY;
        private Point _mouseClickPosition;
        private Point _regionLocation;
        private int _regionWidth;
        private int _regionHeight;
        private Image _displayedImage;
        private ObservableCollection<Image> _imageList;
        private int _imageIndex = 0;
        private BitmapSource _imageSource;
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageClickCommand { get; set; }
        public RelayCommand LeftArrowCommand { get; set; }
        public RelayCommand RightArrowCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseLeftClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseMoveCommand { get; set; }
        private static ITool tool = null;
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
                BoundingBoxWidth = 0;
                BoundingBoxHeight = 0;
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
        public int BoundingBoxWidth
        {
            get
            {
                return _regionWidth;
            }
            set
            {
                _regionWidth = value;
                NotifyPropertyChanged();
            }
        }
        public int BoundingBoxHeight
        {
            get
            {
                return _regionHeight;
            }
            set
            {
                _regionHeight = value;
                NotifyPropertyChanged();
            }
        }
        public Thickness BoundingBoxLocation
        {
            get
            {
                return new Thickness(_regionLocation.X, _regionLocation.Y, 0, 0);
            }
            set
            {
                BoundingBoxLocation = value;
                NotifyPropertyChanged();
            }
        }

        public ImagePresenterViewModel()
        {
            _imageList = new ObservableCollection<Image>();
            _aggregator.GetEvent<DisplayImage>().Subscribe(item => 
            { 
                _imageList = item;
                _imageIndex = 0;
                DisplayedImage = _imageList[_imageIndex];
            });
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageClickExecute);
            LeftArrowCommand = new RelayCommand(PreviousImage);
            RightArrowCommand = new RelayCommand(NextImage);
            MouseLeftClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseLeftClick);
            MouseMoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseMove);
        }

        private void MouseLeftClick(System.Windows.RoutedEventArgs args)
        {
            _mouseClickPosition.X = _mouseX;
            _mouseClickPosition.Y = _mouseY;

            if (Tool.GetToolEnum() == Tools.RegionSelection)
            {
                _regionLocation.X = _mouseX;
                _regionLocation.Y = _mouseY;
                BoundingBoxWidth = 0;
                BoundingBoxHeight = 0;
                NotifyPropertyChanged("BoundingBoxLocation");
                isDragged = true;
            }
            return;
        }
        private void MouseMove(RoutedEventArgs args)
        {
            if (isDragged)
            {
                BoundingBoxWidth = _mouseX - (int)_mouseClickPosition.X;
                if(BoundingBoxWidth < 0)
                {
                    BoundingBoxWidth = Math.Abs(BoundingBoxWidth);
                    _regionLocation.X = _mouseX;
                    NotifyPropertyChanged("BoundingBoxLocation");
                }
                BoundingBoxHeight = _mouseY - (int)_mouseClickPosition.Y;
                if (BoundingBoxHeight < 0)
                {
                    BoundingBoxHeight = Math.Abs(BoundingBoxHeight);
                    _regionLocation.Y = _mouseY;
                    NotifyPropertyChanged("BoundingBoxLocation");
                }
            }
        }

        private void PreviousImage(Object obj)
        {
            _imageIndex = _imageIndex == 0 ? (_imageList.Count - 1) : (_imageIndex - 1);
            DisplayedImage = _imageList[_imageIndex];
        }
        private void NextImage(Object obj)
        {
            _imageIndex = _imageIndex == (_imageList.Count - 1 ) ? 0 : (_imageIndex + 1);
            DisplayedImage = _imageList[_imageIndex];
        }

        private void ImageClickExecute(System.Windows.RoutedEventArgs args)
        {
            if(tool != null)
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Dictionary<String, Object> parameters = new Dictionary<string, object>();
                    switch (Tool.GetToolEnum())
                    {
                        case Tools.None:
                            break;
                        case Tools.RegionSelection:
                            {
                                isDragged = false;
                                parameters.Add("BoundingBoxLocation", new Point(BoundingBoxLocation.Left, BoundingBoxLocation.Top));
                                parameters.Add("BoundingBoxWidth", BoundingBoxWidth);
                                parameters.Add("BoundingBoxHeight", BoundingBoxHeight);
                                parameters.Add("BitmapSource", ImageSource);
                            }
                            break;
                        case Tools.Magnifier:
                            break;
                        case Tools.PixelInformations:
                            {
                                PixelInformationView piv = new PixelInformationView();
                                piv.Show();
                                _aggregator.GetEvent<SendPixelInformationViewEvent>().Publish(piv);
                                parameters.Add("MouseX", _mouseX);
                                parameters.Add("MouseY", _mouseY);
                                parameters.Add("BitmapSource", ImageSource);
                            }
                            break;
                        case Tools.RegionTransformation:
                            break;
                        default:
                            return;
                    }
                    try
                    {
                        tool.AffectImage(parameters);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }));
        }
    }
}
