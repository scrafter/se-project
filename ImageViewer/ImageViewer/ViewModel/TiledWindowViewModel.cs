using ImageViewer.Model;
using ImageViewer.Model.Event;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel
{
    public class TiledWindowViewModel : BaseViewModel
    {
        private ObservableCollection<Image> _imageList;

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
            ImageList = new ObservableCollection<Image>();
            _aggregator.GetEvent<ClearEvent>().Subscribe(Clear);
            _aggregator.GetEvent<FileDialogEvent>().Subscribe(item => { ImageList = item; });
        }

        public void Clear()
        {
            Task.Run(() => ClearAll());
        }

        private void ClearAll()
        {
            foreach (var item in ImageList)
                ImageList.Remove(item);
        }
    }
}
