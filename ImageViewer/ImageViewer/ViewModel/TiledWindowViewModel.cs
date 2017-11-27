using ImageViewer.Methods;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using ImageViewer.View.ImagesWindow;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ImageViewer.ViewModel
{
    public class TiledWindowViewModel : BaseViewModel
    {
        private ObservableCollection<ObservableCollection<Image>> _imageList;
        public RelayCommand DoubleClickCommand { get; set; }
        public RelayCommand RemoveImageCommand { get; set; }
        public GalaSoft.MvvmLight.Command.RelayCommand<DragEventArgs> DragEnterCommand { get; set; }
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG",".JPEG",".TIF",".ICO" };

        public int TiledViewRows
        {
            get
            {
                return _imageList.Count > 15 ? _imageList.Count / 5 + 1 : 3;  
            }
            set { }
        }

        public ObservableCollection<ObservableCollection<Image>> ImageList
        {
            get
            {
                return _imageList;
            }
            set
            {
                _imageList = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TiledViewRows");
            }
        }

        public TiledWindowViewModel()
        {
            DoubleClickCommand = new Methods.RelayCommand(DoubleClickExecute, DoubleClickCanExecute);
            RemoveImageCommand = new Methods.RelayCommand(RemoveImageExecute, RemoveImageCanExecute);
            DragEnterCommand = new GalaSoft.MvvmLight.Command.RelayCommand<System.Windows.DragEventArgs>(FileDragFromWindows);
            ImageList = new ObservableCollection<ObservableCollection<Image>>();
            ImageSaver.SendTheLoadedImages(ImageList);
            _aggregator.GetEvent<ClearEvent>().Subscribe(Clear);
            _aggregator.GetEvent<FileDialogEvent>().Subscribe(item => 
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ObservableCollection<ObservableCollection<Image>> list = new ObservableCollection<ObservableCollection<Image>>();
                    foreach (var image in ImageList)
                    {
                        list.Add(image);
                    }
                    foreach (var image in item)
                    {
                        if(!list.Contains(image))
                            list.Add(image);
                    }
                    ImageList = list;
                    SynchronizeImageExplorer();
                }));
            });
            _aggregator.GetEvent<SendImage>().Subscribe(item => {
                if (ImageList.Contains(item) == false)
                    ImageList.Add(item);
            });
        }
         ~TiledWindowViewModel()
        {
            ImageSaver.SafeToText(ImageList);
        }
        private void RemoveImageExecute(object obj)
        {
            var image = (ObservableCollection<Image>)obj;
            if (image != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ImageList.Remove(image);
                    SynchronizeImageExplorer();
                }));
            }
        }

        private bool RemoveImageCanExecute(object obj)
        {
            return true;
        }

        private void DoubleClickExecute(object obj)
        {
            ObservableCollection<Image> image = (ObservableCollection<Image>)obj;
            if(image !=null)
            {
                DisplayImageWindow displayImageWindow = DisplayImageWindow.Instance;
                displayImageWindow.Show();
                _aggregator.GetEvent<DisplayImage>().Publish(image);
                SynchronizeImageExplorer();
            }
            
        }
        private void SynchronizeImageExplorer()
        {
            _aggregator.GetEvent<SendImageList>().Publish(_imageList);
        }
        private bool DoubleClickCanExecute(object obj)
        {
            return true;
        }

        private void FileDragFromWindows(DragEventArgs obj)
            {
           
            string[] files = (string[])obj.Data.GetData(System.Windows.DataFormats.FileDrop);
            Image image = new Image();
            foreach (string path in files)
                try
                {
                    image.FilePath = path;
                    image.FileName = System.Text.RegularExpressions.Regex.Match(path, @".*\\([^\\]+$)").Groups[1].Value;
                    image.Extension = Path.GetExtension(path);
                    if (image.Extension != "" && image.Extension != ".tmp" && ImageExtensions.Contains(Path.GetExtension(path).ToUpperInvariant()))
                    {
                        ObservableCollection<Image> temp = new ObservableCollection<Image>();
                        temp.Add(image);
                        _aggregator.GetEvent<SendImage>().Publish(temp);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
        }

        public void Clear()
        {
            Task.Run(() => ClearAll());
        }

        private void ClearAll()
        {
           ImageList = new ObservableCollection<ObservableCollection<Image>>();
        }
    }
}
