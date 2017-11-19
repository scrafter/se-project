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

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ImagePresenterViewModel : BaseViewModel
    {
        private int _mouseX;
        private int _mouseY;
        private int _imageWidth;
        private int _imageHeight;
        private Image _displayedImage;
        private BitmapSource _imageSource;
        public GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.RoutedEventArgs> ImageClickCommand { get; set; }
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

        public int ImageWidth
        {
            get
            {
               return _imageWidth;
            }
            set
            {
                _imageWidth = value;
            }
        }

        public int ImageHeight
        {
            get
            {
                return _imageHeight;
            }
            set
            {
                _imageHeight = value;
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
            if(tool != null)
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Dictionary<String, Object> parameters = new Dictionary<string, object>();
                    switch (Tool.GetToolEnum())
                    {
                        case Tools.None:
                            break;
                        case Tools.RegionSelection:
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
                            break;
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
