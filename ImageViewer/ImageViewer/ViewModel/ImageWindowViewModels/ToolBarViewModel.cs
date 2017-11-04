using ImageViewer.Methods;
using ImageViewer.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class ToolBarViewModel : BaseViewModel
    {
        public RelayCommand HideToolBarCommand { get; set; }
        public ToolBarViewModel()
        {
            HideToolBarCommand = new RelayCommand(HideToolBarExecute);
        }

        private void HideToolBarExecute(object obj)
        {
            _aggregator.GetEvent<HideToolbarEvent>().Publish();
        }
    }
}
