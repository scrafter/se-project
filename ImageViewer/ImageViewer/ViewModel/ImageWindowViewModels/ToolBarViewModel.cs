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
        public RelayCommand CreateEditRegionToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }

        public RelayCommand GridOneCommand { get; set; }
        public RelayCommand GridOneToTwoCommand { get; set; }
        public RelayCommand GridTwoToTwoCommand { get; set; }
        public RelayCommand GridThreeToTreeCommand { get; set; }

        public Tools Tool {
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

        private GridStatusEvent.GridStatus _gridStatus;
        public GridStatusEvent.GridStatus GridStatus
        {
            get { return _gridStatus;  }
            set
            {
                _gridStatus = value;
                NotifyPropertyChanged();
            }
        }

        public ToolBarViewModel()
        {
            HideToolBarCommand = new RelayCommand(HideToolBarExecute);
            CreateMagnifyingGlassToolCommand = new RelayCommand(CreateMagnifyingGlassTool);
            CreateEditRegionToolCommand = new RelayCommand(CreateEditRegionTool);
            CreateRegionToolCommand = new RelayCommand(CreateRegionTool);
            CreatePixelPickerToolCommand = new RelayCommand(CreatePixelPickerTool);
            GridOneCommand = new RelayCommand(GridOneExecute);
            GridOneToTwoCommand = new RelayCommand(GridOneToTwoExecute);
            GridTwoToTwoCommand = new RelayCommand(GridTwoToTwoExecute);
            GridThreeToTreeCommand = new RelayCommand(GridThreeToTreeExecute);
            GridStatus = GridStatusEvent.GridStatus.OneToOne;
        }

        private void GridOneExecute(object obj)
        {
            GridStatus = GridStatusEvent.GridStatus.OneToOne;
            _aggregator.GetEvent<GridStatusEvent>().Publish(GridStatusEvent.GridStatus.OneToOne);
        }

        private void GridOneToTwoExecute(object obj)
        {
            GridStatus = GridStatusEvent.GridStatus.OneToTwo;
            _aggregator.GetEvent<GridStatusEvent>().Publish(GridStatusEvent.GridStatus.OneToTwo);
        }

        private void GridTwoToTwoExecute(object obj)
        {
            GridStatus = GridStatusEvent.GridStatus.TwoToTwo;
            _aggregator.GetEvent<GridStatusEvent>().Publish(GridStatusEvent.GridStatus.TwoToTwo);
        }

        private void GridThreeToTreeExecute(object obj)
        {
            GridStatus = GridStatusEvent.GridStatus.ThreeToThree;
            _aggregator.GetEvent<GridStatusEvent>().Publish(GridStatusEvent.GridStatus.ThreeToThree);
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

        private void CreateEditRegionTool(object obj)
        {
            _aggregator.GetEvent<SendToolEvent>().Publish(new EditRegion());
            Tool = Tools.RegionTransformation;
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
