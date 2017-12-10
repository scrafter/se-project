﻿using System;
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
        private bool _isSaving = false;
        public int ViewModelID;
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
        private double _scaleX;
        private double _scaleY;
        private ObservableCollection<Image> _imageList;
        private int _imageIndex = 0;
        private BitmapSource _imageSource;
        private double currentZoomScale = 0.5;
        private double totalZoom = 1;
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageClickCommand { get; set; }
        public RelayCommand LeftArrowCommand { get; set; }
        public RelayCommand RightArrowCommand { get; set; }
        public RelayCommand EscapeCommand { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand SaveRegionCommand { get; set; }
        public RelayCommand SerializeOutputFromListCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageRightClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseLeftClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> MouseMoveCommand { get; set; }
        private ITool _tool = null;
        private Tools _toolType = Tools.None;

        #endregion

        #region Properties

        public bool IsFocused { get; set; }
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
                {
                    ImageSource = DisplayedImage.Bitmap;
                    ImagePosition = DisplayedImage.Position;
                }
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
                return DisplayedImage == null ? new Thickness(0, 0, 0, 0) : DisplayedImage.Position;
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

        public Double ScaleX
        {
            get
            {
                return _scaleX;
            }
            set
            {
                _scaleX = value;
                NotifyPropertyChanged();
            }
        }

        public Double ScaleY
        {
            get
            {
                return _scaleY;
            }
            set
            {
                _scaleY = value;
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

        public ImagePresenterViewModel(ObservableCollection<Image> image,int viewModelID, Tools tool)
        {
            switch (tool)
            {
                case Tools.None:
                    break;
                case Tools.RegionSelection:
                    Tool = new CreateRegion();
                    break;
                case Tools.Magnifier:
                    Tool = new MagnifyingGlass();
                    break;
                case Tools.PixelInformations:
                    Tool = new PixelPicker();
                    break;
                case Tools.RegionTransformation:
                    break;
                case Tools.ImagePan:
                    Tool = new PanImage();
                    break;
                default:
                    break;
            }
            ViewModelID = viewModelID;
            _imageList = new ObservableCollection<Image>();
            _imageList = image;
            _imageIndex = 0;
            DisplayedImage = _imageList[_imageIndex];
            //_aggregator.GetEvent<DisplayImage>().Subscribe(item =>
            //{
            //    _imageList = item;
            //    _imageIndex = 0;
            //    DisplayedImage = _imageList[_imageIndex];
            //});
            _aggregator.GetEvent<SendDisplayedImage>().Subscribe(item =>
            {
                if (item.PresenterID != ViewModelID)
                    return;
                DisplayedImage = item.Image;
            });
            _aggregator.GetEvent<SendToolEvent>().Subscribe(item =>
            {
                Tool = item;
            });
            _aggregator.GetEvent<SendRegionNameEvent>().Subscribe(SaveRegion);
            _aggregator.GetEvent<LoadRegionEvent>().Subscribe(region =>
            {
                if (region.PresenterID != ViewModelID)
                    return;

                if (_imageList != region.ImageList)
                {
                    _imageList = region.ImageList;
                }
                DisplayedImage = region.ImageList.First(x => x == region.AttachedImage);
                RegionLocation = new Thickness(region.Position.X * 96.0 / region.DpiX, region.Position.Y * 96.0 / region.DpiY, 0, 0);
                RegionWidth = (int)(region.Size.Width * 96.0 / region.DpiX);
                RegionHeight = (int)(region.Size.Height * 96.0 / region.DpiY);
                CalculateRegionProperties();
            });
            _aggregator.GetEvent<SendZoomEvent>().Subscribe(zoomingInfo =>
            {

                ScaleX = zoomingInfo.ZoomScale;
                ScaleY = zoomingInfo.ZoomScale;
                ImagePosition = new Thickness(zoomingInfo.ImagePositionX, zoomingInfo.ImagePositionY, -zoomingInfo.ImagePositionX, -zoomingInfo.ImagePositionX);

            });
            _aggregator.GetEvent<SendImageList>().Subscribe(item =>
            {
                try
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
                }
                catch (Exception)
                {

                }
            });
            _aggregator.GetEvent<SynchronizeRegions>().Subscribe(SynchronizeRegion);
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageClickExecute);
            LeftArrowCommand = new RelayCommand(PreviousImage);
            RightArrowCommand = new RelayCommand(NextImage);
            SaveRegionCommand = new RelayCommand(OpenSaveRegionWindow);
            SerializeOutputFromListCommand = new RelayCommand(SerializeOutputFromList);
            EscapeCommand = new RelayCommand(EscapeClicked);
            SelectAllCommand = new RelayCommand(SelectAll);
            MouseLeftClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseLeftClick);
            MouseMoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(MouseMove);
            ImageRightClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs>(ImageRightClickExecute);

            ScaleX = 1;
            ScaleY = 1;

        }
        
        #region Private methods
        private void SynchronizeRegion(SynchronizeRegions sr)
        {
            if(sr.PresenterID != ViewModelID)
            {
                if(sr.Position.Left >= ImageSource.Width || sr.Position.Top >= ImageSource.Height)
                {
                    RegionLocation = new Thickness(0, 0, 0, 0);
                    RegionWidth = 0;
                    RegionHeight = 0;
                }

                RegionWidth = sr.Width;
                RegionHeight = sr.Height;
                int top = (int)sr.Position.Top;
                int left = (int)sr.Position.Left;
                if (top < 0)
                    top = 0;
                if (left < 0)
                    left = 0;
                if (left >= ImageSource.Width)
                    left = (int)ImageSource.Width;
                if (top >= ImageSource.Height)
                    top = (int)ImageSource.Height;
                RegionLocation = new Thickness(left, top, 0, 0);
                if (left + RegionWidth > ImageSource.Width)
                    RegionWidth = (int)ImageSource.Width - left;
                if (top + RegionHeight > ImageSource.Height)
                    RegionHeight = (int)ImageSource.Height - top;

                if(sr.DoProcessing)
                {
                    CreateRegion cr = new CreateRegion();
                    Dictionary<String, Object> parameters = new Dictionary<string, object>();
                    parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
                    parameters.Add("RegionWidth", RegionWidth);
                    parameters.Add("RegionHeight", RegionHeight);
                    parameters.Add("BitmapSource", ImageSource);
                    parameters.Add("ImagePosition", ImagePosition);
                    parameters.Add("PresenterID", ViewModelID);

                    cr.AffectImage(parameters);
                }
            }
        }

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
            if (!_isSaving)
                return;

            Point point = new Point(RegionLocation.Left * ImageSource.DpiY / 96.0, RegionLocation.Top * ImageSource.DpiY / 96.0);
            Region region = new Region(point, new Size(_regionWidth * ImageSource.DpiY / 96.0, _regionHeight * ImageSource.DpiY / 96.0), name, new Vector(ImageSource.DpiX, ImageSource.DpiY), _imageList, _displayedImage, ViewModelID);
            _aggregator.GetEvent<SendRegionEvent>().Publish(region);
            _isSaving = false;
        }
        private void OpenSaveRegionWindow(Object obj)
        {
            _isSaving = true;
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
                        SynchronizeRegions sr = new SynchronizeRegions();
                        sr.PresenterID = ViewModelID;
                        sr.Position = RegionLocation;
                        sr.Width = RegionWidth;
                        sr.Height = RegionHeight;
                        sr.DoProcessing = false;
                        _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
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
                                SynchronizeRegions sr = new SynchronizeRegions();
                                sr.PresenterID = ViewModelID;
                                sr.Position = RegionLocation;
                                sr.Width = RegionWidth;
                                sr.Height = RegionHeight;
                                sr.DoProcessing = false;
                                _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
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

        private void CorrectZoom()
        {
            if (totalZoom > 1)
            {
                totalZoom = 1;
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

        private void ImageRightClickExecute(System.Windows.RoutedEventArgs args)
        {
            if(currentZoomScale < 1 && currentZoomScale > 0)
            {
                currentZoomScale = 1 / currentZoomScale;

                ImageClickExecute(args);

                totalZoom *= currentZoomScale;

                currentZoomScale = 1 / currentZoomScale;
            }

            

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
                                    parameters.Add("PresenterID", ViewModelID);
                                    SynchronizeRegions sr = new SynchronizeRegions();
                                    sr.PresenterID = ViewModelID;
                                    sr.Position = RegionLocation;
                                    sr.Width = RegionWidth;
                                    sr.Height = RegionHeight;
                                    sr.DoProcessing = true;
                                    _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
                                }
                                break;
                            case Tools.Magnifier:
                                {
                                    CorrectZoom();
                                    totalZoom *= currentZoomScale;
                                    parameters.Add("ClickPositionX", _mouseX);
                                    parameters.Add("ClickPositionY", _mouseY);
                                    parameters.Add("DisplayedImage", DisplayedImage);
                                    parameters.Add("ImageWidth", _displayedImage.OriginalBitmap.Width);
                                    parameters.Add("ImageHeight", _displayedImage.OriginalBitmap.Height);
                                    parameters.Add("ZoomValue", totalZoom);
                                }
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
                                    parameters.Add("PresenterID", ViewModelID);
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
