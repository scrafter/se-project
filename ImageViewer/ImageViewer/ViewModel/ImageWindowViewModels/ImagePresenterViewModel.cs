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
        #region Variables
        private bool _isDragged = false;
        private bool _escapeClicked = false;
        private int _mouseX;
        private int _mouseY;
        private int _mouseXDelta = 0;
        private int _mouseYDelta = 0;
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
        public RelayCommand EscapeCommand { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand SaveRegionCommand { get; set; }
        public RelayCommand SerializeOutputFromListCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseLeftClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseMoveCommand { get; set; }
        private ITool _tool = null;
        private Tools _toolType = Tools.None;
        #endregion

        #region Properties
        public ITool Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
                ToolType = _tool == null ? Tools.None : _tool.GetToolEnum();
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
                if (_displayedImage != null)
                    ImageSource = DisplayedImage.Bitmap;
                else
                {
                    ImageSource = null;
                }

                RegionWidth = 0;
                RegionHeight = 0;
                RegionLocation = new Thickness(0, 0, 0, 0);
                NotifyPropertyChanged();
                NotifyPropertyChanged("ImagePosition");
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
        public Thickness ImagePosition
        {
            get
            {
                return DisplayedImage.Position;
            }
            set
            {
                DisplayedImage.Position = value;
                NotifyPropertyChanged();
            }
        }

        public int MouseX
        {
            get { return _mouseX; }
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
        #endregion

        public ImagePresenterViewModel()
        {
            _imageList = new ObservableCollection<Image>();
            _aggregator.GetEvent<DisplayImage>().Subscribe(item =>
            {
                _imageList = item;
                _imageIndex = 0;
                DisplayedImage = _imageList[_imageIndex];
            });
            _aggregator.GetEvent<SendDisplayedImage>().Subscribe(item =>
            {
                DisplayedImage = item;
                NotifyPropertyChanged("DisplayedImage");
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
            _aggregator.GetEvent<SendImageList>().Subscribe(item =>
            {
                if (item.Count != 0)
                {
                    if (!item.Any(x => x == _imageList))
                    {
                        _imageList = item[0];
                        _imageIndex = 0;
                        DisplayedImage = _imageList[0];
                    }
                }
                else
                {
                    DisplayedImage = null;
                    _imageList = null;
                }
            });
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageClickExecute);
            LeftArrowCommand = new RelayCommand(PreviousImage);
            RightArrowCommand = new RelayCommand(NextImage);
            SaveRegionCommand = new RelayCommand(OpenSaveRegionWindow);
            SerializeOutputFromListCommand = new RelayCommand(SerializeOutputFromList);
            EscapeCommand = new RelayCommand(EscapeClicked);
            SelectAllCommand = new RelayCommand(SelectAll);
            MouseLeftClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseLeftClick);
            MouseMoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseMove);
        }
        #region Private methods
        private void SerializeOutputFromList(Object obj)
        {
            OutputSerializer serializer = new OutputSerializer();
            serializer.SerializeList(_imageList, RegionWidth, RegionHeight, RegionLocation);
        }

        private void SelectAll(Object obj)
        {
            int x = (int)ImagePosition.Left;
            int y = (int)ImagePosition.Top;
            if (x < 0)
                x = 0;
            else if (x > ImageSource.PixelWidth)
                return;
            if (y < 0)
                y = 0;
            else if (y > ImageSource.PixelHeight)
                return;

            RegionLocation = new Thickness(x, y, 0, 0);
            RegionWidth = (int)(ImageSource.Width - Math.Abs(ImagePosition.Left));
            RegionHeight = (int)(ImageSource.Height - Math.Abs(ImagePosition.Top));
            ITool tempTool = Tool;
            Tool = new CreateRegion();
            ImageClickExecute(null);
            Tool = tempTool;
        }

        private void EscapeClicked(Object obj)
        {
            _escapeClicked = true;
            _isDragged = false;
            RegionLocation = new Thickness(0, 0, 0, 0);
            RegionHeight = 0;
            RegionWidth = 0;
        }

        private void SaveRegion(String name)
        {
            Point point = new Point(RegionLocation.Left * ImageSource.DpiY / 96.0, RegionLocation.Top * ImageSource.DpiY / 96.0);
            Region region = new Region(point, new Size(_regionWidth * ImageSource.DpiY / 96.0, _regionHeight * ImageSource.DpiY / 96.0), name, new Vector(ImageSource.DpiX, ImageSource.DpiY), _imageList, _displayedImage);
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

            switch (ToolType)
            {
                case Tools.None:
                    break;
                case Tools.RegionSelection:
                    {
                        RegionLocation = new Thickness(_mouseX, _mouseY, 0, 0);
                        RegionWidth = 0;
                        RegionHeight = 0;
                        _isDragged = true;
                    }
                    break;
                case Tools.Magnifier:
                    break;
                case Tools.PixelInformations:
                    break;
                case Tools.RegionTransformation:
                    break;
                case Tools.ImagePan:
                    {
                        _isDragged = true;
                        _mouseXDelta = 0;
                        _mouseYDelta = 0;
                    }
                    break;
                default:
                    break;
            }
            return;
        }
        private void MouseMove(RoutedEventArgs args)
        {
            if (!_escapeClicked)
                if (_isDragged)
                {
                    switch (ToolType)
                    {
                        case Tools.None:
                            break;
                        case Tools.RegionSelection:
                            {
                                RegionWidth = _mouseX - (int)_mouseClickPosition.X;
                                int x = (int)RegionLocation.Left, y = (int)RegionLocation.Top;
                                if (RegionWidth < 0)
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
                            break;
                        case Tools.Magnifier:
                            break;
                        case Tools.PixelInformations:
                            break;
                        case Tools.RegionTransformation:
                            break;
                        case Tools.ImagePan:
                            {
                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    ImageClickExecute(null);
                                    _mouseXDelta = _mouseX - (int)_mouseClickPosition.X;
                                    _mouseYDelta = _mouseY - (int)_mouseClickPosition.Y;
                                    _isDragged = true;
                                }
                                ));
                            }
                            break;
                        default:
                            break;
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
            _imageIndex = _imageIndex == (_imageList.Count - 1) ? 0 : (_imageIndex + 1);
            DisplayedImage = _imageList[_imageIndex];
        }

        private void ImageClickExecute(System.Windows.RoutedEventArgs args)
        {
            if (!_escapeClicked)
            {
                if (_tool != null)
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Dictionary<String, Object> parameters = new Dictionary<string, object>();
                        switch (Tool.GetToolEnum())
                        {
                            case Tools.None:
                                break;
                            case Tools.RegionSelection:
                                {
                                    parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
                                    parameters.Add("RegionWidth", RegionWidth);
                                    parameters.Add("RegionHeight", RegionHeight);
                                    parameters.Add("BitmapSource", ImageSource);
                                    parameters.Add("ImagePosition", ImagePosition);
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
                                    parameters.Add("ImagePosition", ImagePosition);
                                }
                                break;
                            case Tools.RegionTransformation:
                                break;
                            case Tools.ImagePan:
                                {
                                    parameters.Add("MouseClickPosition", _mouseClickPosition);
                                    parameters.Add("MouseX", _mouseX);
                                    parameters.Add("MouseY", _mouseY);
                                    parameters.Add("MouseXDelta", _mouseXDelta);
                                    parameters.Add("MouseYDelta", _mouseYDelta);
                                    parameters.Add("DisplayedImage", DisplayedImage);
                                    parameters.Add("Position", ImagePosition);
                                }
                                break;
                            default:
                                return;
                        }
                        try
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                _tool.AffectImage(parameters);
                            }));
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }));
            }
            else
            {
                _escapeClicked = false;
            }
            _isDragged = false;
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
#endregion
