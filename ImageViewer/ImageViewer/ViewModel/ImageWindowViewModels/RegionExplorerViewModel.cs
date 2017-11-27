﻿using ImageViewer.Methods;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using ImageViewer.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    class RegionExplorerViewModel : BaseViewModel
    {
        private ObservableCollection<Region> _regionList = new ObservableCollection<Region>();
        public RelayCommand RemoveRegionCommand { get; set; }
        public RelayCommand DoubleClickCommand { get; set; }
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
            DoubleClickCommand = new RelayCommand(DoubleClickExecute);
            RemoveRegionCommand = new RelayCommand(RemoveRegion);
            _aggregator.GetEvent<SendRegionEvent>().Subscribe(AddRegion);
            _aggregator.GetEvent<ImageListRemovedEvent>().Subscribe(RemoveRegionOnListRemoval);
        }
        private void RemoveRegionOnListRemoval(ObservableCollection<ObservableCollection<Image>> imageList)
        {
            var list  = _regionList.Where(x => imageList.Contains(x.ImageList));
            var regionList = new ObservableCollection<Region>();
            foreach (var x in list)
            {
                regionList.Add(x);
            }
            RegionList = regionList;
        }
        private void AddRegion(Region region)
        {
            if (region.Save())
            {
                RegionList.Add(region);
                NotifyPropertyChanged("RegionList");
            }
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
        private void DoubleClickExecute(object obj)
        {
            Region region = (Region)obj;
            if (region != null)
            {
                _aggregator.GetEvent<LoadRegionEvent>().Publish(region);
            }

        }
    }
}
