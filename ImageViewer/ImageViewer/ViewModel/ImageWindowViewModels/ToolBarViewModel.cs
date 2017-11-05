﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Methods;
using ImageViewer.Model.Event;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ToolBarViewModel : BaseViewModel
    {
        public RelayCommand HideToolBarCommand { get; set; }
        public RelayCommand CreateMagnifyingGlassToolCommand { get; set; }
        public RelayCommand CreateRegionToolCommand { get; set; }
        public RelayCommand CreateEditRegionToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }



        public ToolBarViewModel()
        {
            HideToolBarCommand = new RelayCommand(HideToolBarExecute);
        }

        private void HideToolBarExecute(object obj)
        {
            _aggregator.GetEvent<HideToolbarEvent>().Publish();
            CreateMagnifyingGlassToolCommand = new RelayCommand(CreateMagnifyingGlassTool);
            CreateEditRegionToolCommand = new RelayCommand(CreateEditRegionTool);
            CreateRegionToolCommand = new RelayCommand(CreateRegionTool);
            CreatePixelPickerToolCommand = new RelayCommand(CreatePixelPickerTool);


        }

        private void CreateMagnifyingGlassTool(object obj)
        {

        }

        private void CreateEditRegionTool(object obj)
        {

        }

        private void CreateRegionTool(object obj)
        {

        }

        private void CreatePixelPickerTool(object obj)
        {

        }

    }
}
