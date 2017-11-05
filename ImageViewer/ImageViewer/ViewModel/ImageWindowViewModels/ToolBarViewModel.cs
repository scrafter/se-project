using ImageViewer.Methods;
using ImageViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ToolBarViewModel : BaseViewModel
    {
        public RelayCommand CreateMagnifyingGlassToolCommand { get; set; }
        public RelayCommand CreateRegionToolCommand { get; set; }
        public RelayCommand CreateEditRegionToolCommand { get; set; }
        public RelayCommand CreatePixelPickerToolCommand { get; set; }



        public ToolBarViewModel()
        {
            CreateMagnifyingGlassToolCommand = new RelayCommand(CreateMagnifyingGlassTool);
            CreateEditRegionToolCommand = new RelayCommand(CreateEditRegionTool);
            CreateRegionToolCommand = new RelayCommand(CreateRegionTool);
            CreatePixelPickerToolCommand = new RelayCommand(CreatePixelPickerTool);


        }

        private void CreateMagnifyingGlassTool(object obj)
        {
            ImagePresenterViewModel.tool = new MagnifyingGlass();
            Console.WriteLine("MagnifyingGlass");

        }

        private void CreateEditRegionTool(object obj)
        {
            Console.WriteLine("EditRegion");
        }

        private void CreateRegionTool(object obj)
        {
            Console.WriteLine("CreateRegion");
        }

        private void CreatePixelPickerTool(object obj)
        {
            Console.WriteLine("PixelPicker");
        }

    }
}
