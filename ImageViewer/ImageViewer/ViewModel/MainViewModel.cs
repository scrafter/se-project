using ImageViewer.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageViewer.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public Visibility FileExplorerVisibility { get; set; }
        protected MainViewModel()
        {
            _aggregator.GetEvent<ClearEvent>().Subscribe(Collapse);
        }

        private void Collapse()
        {
            FileExplorerVisibility = Visibility.Collapsed;
        }

    }
}
