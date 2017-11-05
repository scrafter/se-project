using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Methods;
using ImageViewer.Model.Event;
using ImageViewer.Model;
using ImageViewer.View;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ToolBarViewModel : BaseViewModel
    {
        private string _rGBA;

        public RelayCommand HideToolBarCommand { get; set; }
        public RelayCommand CreateMagnifyingGlassToolCommand { get; set; }
        public RelayCommand CreateRegionToolCommand { get; set; }
        public RelayCommand CreateEditRegionToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }
        public string RGBA {
            get
            {
                return _rGBA;
            }
            set
            {
                _rGBA = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("RGBA");
            } }

        public ToolBarViewModel()
        {
            HideToolBarCommand = new RelayCommand(HideToolBarExecute);
            CreateMagnifyingGlassToolCommand = new RelayCommand(CreateMagnifyingGlassTool);
            CreateEditRegionToolCommand = new RelayCommand(CreateEditRegionTool);
            CreateRegionToolCommand = new RelayCommand(CreateRegionTool);
            CreatePixelPickerToolCommand = new RelayCommand(CreatePixelPickerTool);
            _aggregator.GetEvent<SendPixelInformationEvent>().Subscribe(SetPixelInformations);
        }

        private void SetPixelInformations(Dictionary<String, byte> RGBA)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                byte[] rgba = { RGBA["Red"], RGBA["Green"], RGBA["Blue"], RGBA["Alpha"] };
                this.RGBA = "#" + BitConverter.ToString(rgba).Replace("-", string.Empty);
            }));
        }

        private void HideToolBarExecute(object obj)
        {
            _aggregator.GetEvent<HideToolbarEvent>().Publish();
        }

        private void CreateMagnifyingGlassTool(object obj)
        {
            ImagePresenterViewModel.Tool = new MagnifyingGlass();
        }

        private void CreateEditRegionTool(object obj)
        {
            ImagePresenterViewModel.Tool = new EditRegion();
        }

        private void CreateRegionTool(object obj)
        {
            ImagePresenterViewModel.Tool = new CreateRegion();
        }

        private void CreatePixelPickerTool(object obj)
        {
            ImagePresenterViewModel.Tool = new PixelPicker();
        }

    }
}
