using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Methods;
using ImageViewer.Model.Event;
using ImageViewer.Model;
using ImageViewer.View;
using System.Windows.Media;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ToolBarViewModel : BaseViewModel
    {
        private Tools _tool = Tools.None;
        public RelayCommand HideToolBarCommand { get; set; }
        public RelayCommand CreateMagnifyingGlassToolCommand { get; set; }
        public RelayCommand CreateRegionToolCommand { get; set; }
        public RelayCommand PanImageToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }

        public Tools Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
                NotifyPropertyChanged();
            }
        }
        public ToolBarViewModel()
        {
            HideToolBarCommand = new RelayCommand(HideToolBarExecute);
            CreateMagnifyingGlassToolCommand = new RelayCommand(CreateMagnifyingGlassTool);
            PanImageToolCommand = new RelayCommand(PanImageTool);
            CreateRegionToolCommand = new RelayCommand(CreateRegionTool);
            CreatePixelPickerToolCommand = new RelayCommand(CreatePixelPickerTool);
        }

        private void HideToolBarExecute(object obj)
        {
            _aggregator.GetEvent<HideToolbarEvent>().Publish();
        }

        private void CreateMagnifyingGlassTool(object obj)
        {
            _aggregator.GetEvent<SendToolEvent>().Publish(new MagnifyingGlass());
            Tool = Tools.Magnifier;
        }

        private void PanImageTool(object obj)
        {
            _aggregator.GetEvent<SendToolEvent>().Publish(new PanImage());
            Tool = Tools.ImagePan;
        }

        private void CreateRegionTool(object obj)
        {
            _aggregator.GetEvent<SendToolEvent>().Publish(new CreateRegion());
            Tool = Tools.RegionSelection;
        }

        private void CreatePixelPickerTool(object obj)
        {
            _aggregator.GetEvent<SendToolEvent>().Publish(new PixelPicker());
            Tool = Tools.PixelInformations;
        }
    }
}
