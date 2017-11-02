using ImageViewer.Methods;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using ImageViewer.View.ImagesWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageViewer.ViewModel
{
    public class TiledWindowViewModel : BaseViewModel
    {
        private ObservableCollection<Image> _imageList;

        public RelayCommand DoubleClickCommand { get; set; }
        public RelayCommand RemoveImageCommand { get; set; }
        public ObservableCollection<Image> ImageList
        {
            get => _imageList;
            set
            {
                _imageList = value;
                NotifyPropertyChanged();
            }
        }

        public TiledWindowViewModel()
        {
            DoubleClickCommand = new RelayCommand(DoubleClickExecute, DoubleClickCanExecute);
            RemoveImageCommand = new RelayCommand(RemoveImageExecute, RemoveImageCanExecute);
            ImageList = new ObservableCollection<Image>();
            _aggregator.GetEvent<ClearEvent>().Subscribe(Clear);
            _aggregator.GetEvent<FileDialogEvent>().Subscribe(item => { ImageList = item; });
            _aggregator.GetEvent<SendImage>().Subscribe(item => {
                if (ImageList.Contains(item) == false)
                    ImageList.Add(item);
            });
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
                //if(_displayImageWindow == null)
                DisplayImageWindow displayImageWindow = new DisplayImageWindow();
                displayImageWindow.Show();
                _aggregator.GetEvent<DisplayImage>().Publish(image);
            }
            
        }

        private bool DoubleClickCanExecute(object obj)
        {
            return true;
        }

        public void Clear()
        {
            Task.Run(() => ClearAll());
        }

        private void ClearAll()
        {
            App.Current.Dispatcher.Invoke(new Action(()=>
            {
                foreach (var item in ImageList)
                    ImageList.Remove(item);
            }));
        }
    }
}
