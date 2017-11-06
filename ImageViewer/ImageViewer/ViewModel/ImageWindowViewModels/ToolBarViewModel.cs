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
        private Brush _pixelColor;
        private Tools _tool = Tools.None;

        public RelayCommand HideToolBarCommand { get; set; }
        public RelayCommand CreateMagnifyingGlassToolCommand { get; set; }
        public RelayCommand CreateRegionToolCommand { get; set; }
        public RelayCommand CreateEditRegionToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }
        public Brush PixelColor   {
            get
            {
                return _pixelColor;
            }
            set
            {
                _pixelColor = value;
                NotifyPropertyChanged();
            } }

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
                byte[] rgba = { RGBA["Alpha"], RGBA["Red"], RGBA["Green"], RGBA["Blue"] };
                Color c = Color.FromArgb(RGBA["Alpha"], RGBA["Red"], RGBA["Green"], RGBA["Blue"]);
                this.PixelColor = new SolidColorBrush(c);
            }));
        }

        private void HideToolBarExecute(object obj)
        {
            _aggregator.GetEvent<HideToolbarEvent>().Publish();
        }

        private void CreateMagnifyingGlassTool(object obj)
        {
            ImagePresenterViewModel.Tool = new MagnifyingGlass();
            Tool = Tools.Magnifier;
        }

        private void CreateEditRegionTool(object obj)
        {
            ImagePresenterViewModel.Tool = new EditRegion();
            Tool = Tools.RegionTransformation;
        }

        private void CreateRegionTool(object obj)
        {
            ImagePresenterViewModel.Tool = new CreateRegion();
            Tool = Tools.RegionSelection;
        }

        private void CreatePixelPickerTool(object obj)
        {
            ImagePresenterViewModel.Tool = new PixelPicker();
            Tool = Tools.PixelInformations;
        }

    }
}
