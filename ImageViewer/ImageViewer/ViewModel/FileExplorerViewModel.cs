using ImageViewer.Methods;
using ImageViewer.Model.Event;
using ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace ImageViewer.ViewModel
{
    public class FileExplorerViewModel : BaseViewModel
    {
        public RelayCommand CollapseCommand { get; set; }

        public FileExplorerViewModel()
        {
            CollapseCommand = new RelayCommand(CollapseExecute, CollapseCanExecute);
        }

        private void CollapseExecute(object obj)
        {
            CollapseMethod();
            //Task.Run(() => CollapseMethod());
        }

        private void CollapseMethod()
        {
            _aggregator.GetEvent<CollapseEvent>().Publish();
        }

        private bool CollapseCanExecute(object obj)
        {
            return true;
        }
    }
}
