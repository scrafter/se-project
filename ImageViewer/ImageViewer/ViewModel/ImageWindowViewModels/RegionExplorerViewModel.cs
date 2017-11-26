using ImageViewer.Methods;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    class RegionExplorerViewModel : BaseViewModel
    {
        private ObservableCollection<Region> _regionList = new ObservableCollection<Region>();
        public RelayCommand RemoveRegionCommand { get; set; }
        public ObservableCollection<Region> RegionList
        {
            get
            {
                return _regionList;
            }
            set
            {
                _regionList = value;
                NotifyPropertyChanged();
            }
        }

        public RegionExplorerViewModel()
        {
            RemoveRegionCommand = new RelayCommand(RemoveRegion);
            _aggregator.GetEvent<SendRegionEvent>().Subscribe(AddRegion);
        }

        private void AddRegion(Region region)
        {
            RegionList.Add(region);
            NotifyPropertyChanged("RegionList");
        }
        private void RemoveRegion(Object obj)
        {
            var region = (Region)obj;
            if (region != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    RegionList.Remove(region);
                }));
            }
        }
    }
}
