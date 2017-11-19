using ImageViewer.Methods;
using GalaSoft.MvvmLight.Command;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using ImageViewer.View.ImagesWindow;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ImageViewer.ViewModel
{
    public class TiledWindowViewModel : BaseViewModel
    {
        private ObservableCollection<Image> _imageList;
        public Methods.RelayCommand DoubleClickCommand { get; set; }
        public Methods.RelayCommand RemoveImageCommand { get; set; }
        public RelayCommand<System.Windows.DragEventArgs> DragEnterCommand { get; set; }
        public static readonly System.Collections.Generic.List<string> ImageExtensions = new System.Collections.Generic.List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG",".JPEG",".TIF",".ICO" };

        public int TiledViewRows
        {
            get
            {
                return _imageList.Count > 15 ? _imageList.Count / 5 + 1 : 3;  
            }
            set { }
        }

        public ObservableCollection<Image> ImageList
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
            ImageList = new ObservableCollection<Image>();
            ImageSaver.SendTheLoadedImages(ImageList);
            _aggregator.GetEvent<ClearEvent>().Subscribe(Clear);
            _aggregator.GetEvent<FileDialogEvent>().Subscribe(item => 
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ObservableCollection<Image> list = new ObservableCollection<Image>();
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
            var image = (Image)obj;
            if (image != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ImageList.Remove(image);
                }));
            }
        }

        private bool RemoveImageCanExecute(object obj)
        {
            return true;
        }

        private void DoubleClickExecute(object obj)
        {
            var image = (Image)obj;
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
            Model.Image image = new Model.Image();
            foreach (string path in files)
                try
                {
                        image.FilePath = path;
                    image.FileName = System.Text.RegularExpressions.Regex.Match(path, @".*\\([^\\]+$)").Groups[1].Value;
                    image.Extension = Path.GetExtension(path);
                    if (image.Extension != "" && image.Extension != ".tmp" && ImageExtensions.Contains(Path.GetExtension(path).ToUpperInvariant()))
                        _aggregator.GetEvent<SendImage>().Publish(image);
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
           ImageList = new ObservableCollection<Image>();
        }
    }
}
