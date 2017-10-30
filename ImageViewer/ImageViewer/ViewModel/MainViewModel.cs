using ImageViewer.Model.Event;
using ImageViewer.View;
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
        private Visibility fileExplorerVisibility;
        public Visibility FileExplorerVisibility {
            get => fileExplorerVisibility;
            set
            {
                fileExplorerVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public MainViewModel()
        {
            _aggregator.GetEvent<CollapseEvent>().Subscribe(Collapse);
        }

        public void Collapse()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                FileExplorerVisibility = Visibility.Collapsed;
            }));
            
        }

    }
}
