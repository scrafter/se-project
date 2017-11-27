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
        private Thickness _regionLocation;
        private int _regionWidth;
        private int _regionHeight;
        private Image _displayedImage;
        private ObservableCollection<Image> _imageList;
        private int _imageIndex = 0;
        private BitmapSource _imageSource;
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageClickCommand { get; set; }
        public RelayCommand LeftArrowCommand { get; set; }
        public RelayCommand RightArrowCommand { get; set; }
        public RelayCommand SaveRegionCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseLeftClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseMoveCommand { get; set; }
        private  ITool _tool = null;
        private Tools _toolType = Tools.None;
        public  ITool Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
                ToolType = _tool.GetToolEnum();
                NotifyPropertyChanged();
            }
        }
        public Tools ToolType
        {
            get
            {
                return _toolType;
            }
            set
            {
                _toolType = value;
                NotifyPropertyChanged();
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
                RegionWidth = 0;
                RegionHeight = 0;
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
        public int RegionWidth
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
        public int RegionHeight
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
        public Thickness RegionLocation
        {
            get
            {
                return _regionLocation;
            }
            set
            {
                _regionLocation = value;
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
            _aggregator.GetEvent<SendToolEvent>().Subscribe(item =>
            {
                Tool = item;
            });
            _aggregator.GetEvent<SendRegionNameEvent>().Subscribe(SaveRegion);
            _aggregator.GetEvent<LoadRegionEvent>().Subscribe(region =>
            {
                if (_imageList != region.ImageList)
                {
                    _imageList = region.ImageList;
                    DisplayedImage = region.ImageList.First(x => x == region.AttachedImage);
                }
                RegionLocation = new Thickness(region.Position.X * 96.0 / region.DpiX, region.Position.Y * 96.0 / region.DpiY, 0, 0);
                RegionWidth = (int)(region.Size.Width * 96.0 / region.DpiX);
                RegionHeight = (int)(region.Size.Height * 96.0 / region.DpiY);
                CalculateRegionProperties();
            });
            _aggregator.GetEvent<ImageListRemovedEvent>().Subscribe(item =>
            {
                _imageList = item[0];
                _imageIndex = 0;
                DisplayedImage = _imageList[0];
            });
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageClickExecute);
            LeftArrowCommand = new RelayCommand(PreviousImage);
            RightArrowCommand = new RelayCommand(NextImage);
            SaveRegionCommand = new RelayCommand(OpenSaveRegionWindow);
            MouseLeftClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseLeftClick);
            MouseMoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseMove);
        }

        private void SaveRegion(String name)
        {
            Point point = new Point(RegionLocation.Left * ImageSource.DpiY / 96.0, RegionLocation.Top * ImageSource.DpiY / 96.0);
            Region region = new Region(point , new Size(_regionWidth * ImageSource.DpiY / 96.0, _regionHeight * ImageSource.DpiY / 96.0), name, new Vector(ImageSource.DpiX, ImageSource.DpiY), _imageList, _displayedImage);
            _aggregator.GetEvent<SendRegionEvent>().Publish(region);
        }
        private void OpenSaveRegionWindow(Object obj)
        {
            SaveRegionWindow.Instance.Show();
        }

        private void MouseLeftClick(System.Windows.RoutedEventArgs args)
        {
            _mouseClickPosition.X = _mouseX;
            _mouseClickPosition.Y = _mouseY;

            if (_toolType == Tools.RegionSelection)
            {
                RegionLocation = new Thickness(_mouseX, _mouseY, 0, 0);
                RegionWidth = 0;
                RegionHeight = 0;
                isDragged = true;
            }
            return;
        }
        private void MouseMove(RoutedEventArgs args)
        {
            if (isDragged)
            {
                RegionWidth = _mouseX - (int)_mouseClickPosition.X;
                int x = (int)RegionLocation.Left, y = (int)RegionLocation.Top;
                if(RegionWidth < 0)
                {
                    RegionWidth = Math.Abs(RegionWidth);
                    x = _mouseX;
                }
                RegionHeight = _mouseY - (int)_mouseClickPosition.Y;
                if (RegionHeight < 0)
                {
                    RegionHeight = Math.Abs(RegionHeight);
                    y = _mouseY;
                }
                RegionLocation = new Thickness(x, y, 0, 0);
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
            if(_tool != null)
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
                                parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
                                parameters.Add("RegionWidth", RegionWidth);
                                parameters.Add("RegionHeight", RegionHeight);
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
                        _tool.AffectImage(parameters);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }));
        }
        private void CalculateRegionProperties()
        {
            Dictionary<String, Object> parameters = new Dictionary<string, object>();
            parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
            parameters.Add("RegionWidth", RegionWidth);
            parameters.Add("RegionHeight", RegionHeight);
            parameters.Add("BitmapSource", ImageSource);
            ITool tool = new CreateRegion();
            tool.AffectImage(parameters);
        }
    }
}
