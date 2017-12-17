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
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ImagePresenterViewModel : BaseViewModel
    {
        #region Variables
        private String _imageSize;
        private Thread _regionThread;
        private Dictionary<Type, SubscriptionToken> _subscriptionTokens = new Dictionary<Type, SubscriptionToken>();
        private bool _isSaving = false;
        private bool _isSynchronized = true;
        private bool _isDragged = false;
        private bool _escapeClicked = false;
        private int ViewModelID;
        private int MaxWindows;
        private int _mouseX;
        private int _mouseY;
        private int _mouseXDelta = 0;
        private int _mouseYDelta = 0;
        private int _regionWidth;
        private int _regionHeight;
        private int _imageIndex = 0;
        private Point _mouseClickPosition;
        private Thickness _regionLocation;
        private Image _displayedImage;
        private ObservableCollection<Image> _imageList;
        private BitmapSource _imageSource;
        private double _scale;
        private double _zoomStep = 1.1;
        private ITool _tool = null;
        private Tools _toolType = Tools.None;

        public RelayCommand LeftArrowCommand { get; set; }
        public RelayCommand RightArrowCommand { get; set; }
        public RelayCommand EscapeCommand { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand SaveRegionCommand { get; set; }
        public RelayCommand SerializeOutputFromListCommand { get; set; }
        public RelayCommand ResetPositionCommand { get; set; }
        public RelayCommand ResetZoomCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs> MouseLeftClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs> MouseMoveCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs> MouseOverCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs> ImageClickCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<MouseWheelEventArgs> MouseWheelCommand { get; set; }


        #endregion
        #region Properties
        public int ImageIndex
        {
            get
            {
                return _imageIndex;
            }
            set
            {
                _imageIndex = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsSynchronized
        {
            get
            {
                return _isSynchronized;
            }
            set
            {
                _isSynchronized = value;
                if (value == true)
                {
                    SubscriptionToken token;
                    if (!_subscriptionTokens.ContainsKey(typeof(SynchronizeImagePositions)))
                    {
                        token = _aggregator.GetEvent<SynchronizeImagePositions>().Subscribe(SynchronizeImagePosition);
                        _subscriptionTokens.Add(typeof(SynchronizeImagePositions), token);
                    }

                    if (!_subscriptionTokens.ContainsKey(typeof(SynchronizeRegions)))
                    {
                        token = _aggregator.GetEvent<SynchronizeRegions>().Subscribe(SynchronizeRegion);
                        _subscriptionTokens.Add(typeof(SynchronizeRegions), token);
                    }

                    if (!_subscriptionTokens.ContainsKey(typeof(ZoomEvent)))
                    {
                        token = _aggregator.GetEvent<ZoomEvent>().Subscribe(SynchronizeZoom);
                        _subscriptionTokens.Add(typeof(ZoomEvent), token);
                    }

                    if (!_subscriptionTokens.ContainsKey(typeof(NextPreviousImageEvent)))
                    {
                        token = _aggregator.GetEvent<NextPreviousImageEvent>().Subscribe(arg =>
                        {
                            if (arg.PresenterID != ViewModelID)
                            {
                                if (arg.ToNext)
                                {
                                    ImageIndex = _imageIndex == (_imageList.Count - 1) ? 0 : (_imageIndex + 1);
                                    DisplayedImage = _imageList[_imageIndex];
                                }
                                else
                                {
                                    ImageIndex = _imageIndex == 0 ? (_imageList.Count - 1) : (_imageIndex - 1);
                                    DisplayedImage = _imageList[_imageIndex];
                                }
                            }
                        });
                        _subscriptionTokens.Add(typeof(NextPreviousImageEvent), token);
                    }

                }
                else
                {
                    _aggregator.GetEvent<SynchronizeImagePositions>().Unsubscribe(_subscriptionTokens[typeof(SynchronizeImagePositions)]);
                    _subscriptionTokens.Remove(typeof(SynchronizeImagePositions));
                    _aggregator.GetEvent<SynchronizeRegions>().Unsubscribe(_subscriptionTokens[typeof(SynchronizeRegions)]);
                    _subscriptionTokens.Remove(typeof(SynchronizeRegions));
                    _aggregator.GetEvent<NextPreviousImageEvent>().Unsubscribe(_subscriptionTokens[typeof(NextPreviousImageEvent)]);
                    _subscriptionTokens.Remove(typeof(NextPreviousImageEvent));
                    _aggregator.GetEvent<ZoomEvent>().Unsubscribe(_subscriptionTokens[typeof(ZoomEvent)]);
                    _subscriptionTokens.Remove(typeof(ZoomEvent));
                }
                NotifyPropertyChanged();
            }
        }
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
        public String ImageSize
        {
            get
            {
                return _imageSize;
            }
            set
            {
                _imageSize = value;
                NotifyPropertyChanged();
            }
        }

        public String ImagePath
        {
            get
            {
                return DisplayedImage != null ? DisplayedImage.FilePath : String.Empty;
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
                    ImageSize = $"{ImageSource.PixelWidth} x {ImageSource.PixelHeight}";
                    CalculateRegionProperties();
                }
                else
                {
                    ImageSource = null;
                    RegionLocation = new Thickness(0, 0, 0, 0);
                    RegionWidth = 0;
                    RegionHeight = 0;
                    ImageSize = String.Empty;
                    Tool = null;
                    ToolType = Tools.None;
                }
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
                if (DisplayedImage == null)
                    return;
                DisplayedImage.Position = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("DisplayedImage");
            }
        }
        public int MouseX
        {
            get { return _mouseX; }
            set
            {
                _mouseX = value;
                NotifyPropertyChanged();
            }
        }
        public int MouseY
        {
            get { return _mouseY; ; }
            set
            {
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

        public double Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                NotifyPropertyChanged();
            }
        }



        #endregion

        public ImagePresenterViewModel(ObservableCollection<Image> image, int viewModelID, Tools tool, int maxWindows)
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
                case Tools.Rotate:
                    Tool = new Rotate();
                    break;
                default:
                    break;
            }
            ViewModelID = viewModelID;
            MaxWindows = maxWindows;
            _imageList = new ObservableCollection<Image>();
            _imageList = image;
            ImageIndex = 0;
            DisplayedImage = _imageList[_imageIndex];
            IsSynchronized = true;
            Scale = 1;
            _aggregator.GetEvent<SerializeOutputEvent>().Subscribe(SerializeOutputFromPresenters);
            _aggregator.GetEvent<SynchronizationEvent>().Subscribe(i =>
            {
                if (ViewModelID == i)
                    IsSynchronized = !IsSynchronized;
            });
            _aggregator.GetEvent<SendDisplayedImage>().Subscribe(item =>
            {
                if ((item.PresenterID == ViewModelID || item.DoReset == true) && item.Image != null)
                    ImagePosition = item.Image.Position;
                else if (item.IsSynchronized == true && IsSynchronized && item.DoReset == false)
                {
                    ImagePosition = new Thickness(ImagePosition.Left + item.OffsetX, ImagePosition.Top + item.OffsetY, -(ImagePosition.Left + item.OffsetX), -(ImagePosition.Top + item.OffsetY));
                }
                if (item.DoProcessing)
                {
                    CalculateRegionProperties();
                }
                    
            });
            _aggregator.GetEvent<SendToolEvent>().Subscribe(item =>
            {
                Tool = item;
            });
            _aggregator.GetEvent<SendRegionNameEvent>().Subscribe(SaveRegion);
            _aggregator.GetEvent<LoadRegionEvent>().Subscribe(region =>
            {
                ImagePosition = region.ImagePosition;
                if (region.PresenterID > MaxWindows)
                {
                    region.PresenterID = DisplayImageWindowViewModel.ImageCounter;
                }
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
                ImageIndex = region.ImageIndex;
                if (IsSynchronized)
                {
                    SynchronizeRegions sr = new SynchronizeRegions();
                    sr.PresenterID = ViewModelID;
                    sr.Position = RegionLocation;
                    sr.Width = RegionWidth;
                    sr.Height = RegionHeight;
                    sr.DoProcessing = true;
                    _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
                }
                CalculateRegionProperties();
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
                            ImageIndex = 0;
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
            ImageClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs>(ImageClickExecute);
            LeftArrowCommand = new RelayCommand(PreviousImage);
            RightArrowCommand = new RelayCommand(NextImage);
            SaveRegionCommand = new RelayCommand(OpenSaveRegionWindow);
            SerializeOutputFromListCommand = new RelayCommand(SerializeOutputFromList);
            EscapeCommand = new RelayCommand(EscapeClicked);
            SelectAllCommand = new RelayCommand(SelectAll);
            ResetPositionCommand = new RelayCommand(ResetPosition);
            ResetZoomCommand = new RelayCommand(ResetZoom);
            MouseLeftClickCommand = new GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs>(MouseLeftClick);
            MouseMoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs>(MouseMove);
            MouseOverCommand = new GalaSoft.MvvmLight.Command.RelayCommand<RoutedEventArgs>(MouseEnter);
            MouseWheelCommand = new GalaSoft.MvvmLight.Command.RelayCommand<MouseWheelEventArgs>(MouseWheel);
        }

        #region Private methods

        private void ResetZoom(Object arg)
        {
            Scale = 1;
            if (IsSynchronized)
                _aggregator.GetEvent<ZoomEvent>().Publish(new ZoomEvent(Scale, ViewModelID));
        }
        private void ResetPosition(Object arg)
        {
            ImagePosition = new Thickness(0, 0, 0, 0);
            SendDisplayedImage sdi = new SendDisplayedImage();
            sdi.IsSynchronized = IsSynchronized;
            sdi.PresenterID = ViewModelID;
            sdi.Image = DisplayedImage;
            sdi.DoReset = true;
            _aggregator.GetEvent<SendDisplayedImage>().Publish(sdi);
        }
        private void SynchronizeImagePosition(Dictionary<String, Object> parameters)
        {
            if ((int)parameters["PresenterID"] == ViewModelID)
                return;
            Dictionary<String, Object> args = new Dictionary<String, Object>(parameters);
            args["DisplayedImage"] = DisplayedImage;
            args["Position"] = ImagePosition;
            args["PresenterID"] = ViewModelID;
            PanImage pi = new PanImage();
            pi.AffectImage(args);
        }
        private void MouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta < 0)
            {
                if (Scale > 0.1)
                {
                    Scale /= _zoomStep;
                }

            }
            else
            {
                if (Scale <= 8.0)
                {
                    Scale *= _zoomStep;
                }
            }

            ImagePosition = new Thickness(ImagePosition.Left, ImagePosition.Top, ImagePosition.Right / Scale, ImagePosition.Bottom / Scale);

            if (IsSynchronized)
                _aggregator.GetEvent<ZoomEvent>().Publish(new ZoomEvent(Scale, ViewModelID));


        }
        private void MouseEnter(RoutedEventArgs e)
        {
            CalculateRegionProperties();
            _aggregator.GetEvent<SendPresenterIDEvent>().Publish(ViewModelID);
        }

        private void SynchronizeZoom(ZoomEvent ze)
        {
            if (ze.ViewModelID != this.ViewModelID)
            {
                this.Scale = ze.Zoom;
                ImagePosition = new Thickness(ImagePosition.Left, ImagePosition.Top, ImagePosition.Right / Scale, ImagePosition.Bottom / Scale);
            }
        }

        private void SynchronizeRegion(SynchronizeRegions sr)
        {
            if (sr.PresenterID != ViewModelID)
            {
                RegionWidth = sr.Width;
                RegionHeight = sr.Height;
                int top = (int)sr.Position.Top;
                int left = (int)sr.Position.Left;
                RegionLocation = new Thickness(left, top, 0, 0);

                if (sr.DoProcessing)
                {
                    CreateRegion cr = new CreateRegion();
                    Dictionary<String, Object> parameters = new Dictionary<string, object>();
                    parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
                    parameters.Add("RegionWidth", RegionWidth);
                    parameters.Add("RegionHeight", RegionHeight);
                    parameters.Add("BitmapSource", ImageSource);
                    parameters.Add("ImagePosition", ImagePosition);
                    parameters.Add("PresenterID", ViewModelID);
                    parameters.Add("Scale", Scale);

                    cr.AffectImage(parameters);
                }
            }
        }
        private void SerializeOutputFromPresenters(string path)
        {
            OutputSerializer os = new OutputSerializer();
            os.SaveByRegion(DisplayedImage, ViewModelID, RegionWidth, RegionHeight, RegionLocation, path, Scale);
        }
        private void SerializeOutputFromList(Object obj)
        {
            OutputSerializer serializer = new OutputSerializer();
            serializer.SerializeList(_imageList, RegionWidth, RegionHeight, RegionLocation, Scale);
        }
        private void SelectAll(Object obj)
        {
            if (DisplayedImage == null)
                return;
            int x = (int)(ImagePosition.Left);
            int y = (int)(ImagePosition.Top);

            RegionLocation = new Thickness(x, y, 0, 0);
            RegionWidth = (int)(ImageSource.Width * Scale);
            RegionHeight = (int)(ImageSource.Height * Scale);
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
            Region region = new Region(point, new Size(_regionWidth * ImageSource.DpiY / 96.0, _regionHeight * ImageSource.DpiY / 96.0), name, new Vector(ImageSource.DpiX, ImageSource.DpiY), _imageList, _displayedImage, ViewModelID, ImageIndex);
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
                        if (IsSynchronized)
                        {
                            SynchronizeRegions sr = new SynchronizeRegions();
                            sr.PresenterID = ViewModelID;
                            sr.Position = RegionLocation;
                            sr.Width = RegionWidth;
                            sr.Height = RegionHeight;
                            sr.DoProcessing = false;
                            _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
                        }
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
            if (DisplayedImage == null)
                return;
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
                                if (IsSynchronized)
                                {
                                    SynchronizeRegions sr = new SynchronizeRegions();
                                    sr.PresenterID = ViewModelID;
                                    sr.Position = RegionLocation;
                                    sr.Width = RegionWidth;
                                    sr.Height = RegionHeight;
                                    sr.DoProcessing = false;
                                    _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
                                }
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
                                    Dictionary<String, Object> parameters = new Dictionary<string, object>();
                                    parameters.Add("MouseClickPosition", _mouseClickPosition);
                                    parameters.Add("MouseX", MouseX);
                                    parameters.Add("MouseY", MouseY);
                                    parameters.Add("MouseXDelta", _mouseXDelta);
                                    parameters.Add("MouseYDelta", _mouseYDelta);
                                    parameters.Add("DisplayedImage", DisplayedImage);
                                    parameters.Add("Position", ImagePosition);
                                    parameters.Add("PresenterID", ViewModelID);
                                    parameters.Add("IsSynchronized", IsSynchronized);
                                    parameters.Add("DoProcessing", false);
                                    Tool.AffectImage(parameters);
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
            ImageIndex = _imageIndex == 0 ? (_imageList.Count - 1) : (_imageIndex - 1);
            DisplayedImage = _imageList[_imageIndex];
            if (IsSynchronized)
                _aggregator.GetEvent<NextPreviousImageEvent>().Publish(new NextPreviousImageEvent(false, ViewModelID));
        }
        private void NextImage(Object obj)
        {
            ImageIndex = _imageIndex == (_imageList.Count - 1) ? 0 : (_imageIndex + 1);
            DisplayedImage = _imageList[_imageIndex];
            if (IsSynchronized)
                _aggregator.GetEvent<NextPreviousImageEvent>().Publish(new NextPreviousImageEvent(true, ViewModelID));
        }
        private void ImageClickExecute(System.Windows.RoutedEventArgs args)
        {
            if (DisplayedImage == null)
                return;
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
                                    parameters.Add("Scale", Scale);
                                    if (IsSynchronized)
                                    {
                                        SynchronizeRegions sr = new SynchronizeRegions();
                                        sr.PresenterID = ViewModelID;
                                        sr.Position = RegionLocation;
                                        sr.Width = RegionWidth;
                                        sr.Height = RegionHeight;
                                        sr.DoProcessing = true;
                                        _aggregator.GetEvent<SynchronizeRegions>().Publish(sr);
                                    }
                                }
                                break;
                            case Tools.Magnifier:
                                break;
                            case Tools.PixelInformations:
                                {
                                    PixelInformationView piv = new PixelInformationView();
                                    piv.Show();
                                    _aggregator.GetEvent<SendPixelInformationViewEvent>().Publish(piv);
                                    parameters.Add("MouseX", MouseX);
                                    parameters.Add("MouseY", MouseY);

                                    parameters.Add("Scale", Scale);

                                    parameters.Add("BitmapSource", ImageSource);
                                    parameters.Add("ImagePosition", ImagePosition);
                                }
                                break;
                            case Tools.RegionTransformation:
                                break;
                            case Tools.ImagePan:
                                {
                                    parameters.Add("MouseClickPosition", _mouseClickPosition);
                                    parameters.Add("MouseX", MouseX);
                                    parameters.Add("MouseY", MouseY);
                                    parameters.Add("MouseXDelta", _mouseXDelta);
                                    parameters.Add("MouseYDelta", _mouseYDelta);
                                    parameters.Add("DisplayedImage", DisplayedImage);
                                    parameters.Add("Position", ImagePosition);
                                    parameters.Add("PresenterID", ViewModelID);
                                    parameters.Add("IsSynchronized", IsSynchronized);
                                    parameters.Add("DoProcessing", true);
                                }
                                break;
                            case Tools.Rotate:
                                {
                                    parameters.Add("DisplayedImage", DisplayedImage);
                                    parameters.Add("PresenterID", ViewModelID);
                                }
                                break;

                            default:
                                return;
                        }
                        try
                        {
                            _tool.AffectImage(parameters);
                            if (ToolType == Tools.ImagePan)
                            {
                                CalculateRegionProperties();
                            }
                                
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
            if (DisplayedImage == null)
                return;
            Dictionary<String, Object> parameters = new Dictionary<string, object>();
            parameters.Add("RegionLocation", new Point(RegionLocation.Left, RegionLocation.Top));
            parameters.Add("RegionWidth", RegionWidth);
            parameters.Add("RegionHeight", RegionHeight);
            parameters.Add("BitmapSource", ImageSource);
            parameters.Add("ImagePosition", ImagePosition);
            parameters.Add("PresenterID", ViewModelID);
            parameters.Add("Scale", Scale);
            ITool tool = new CreateRegion();
            tool.AffectImage(parameters);
        }
        #endregion
    }
}

