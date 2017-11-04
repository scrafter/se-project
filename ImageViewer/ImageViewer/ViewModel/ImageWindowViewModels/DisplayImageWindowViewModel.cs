using ImageViewer.Methods;
using ImageViewer.Model.Event;
using ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class DisplayImageWindowViewModel : BaseViewModel
    {
        public RelayCommand ShowToolbarCommand { get; set; }
        private Visibility toolBarVisibility;
        public Visibility ToolBarVisibility
        {
            get => toolBarVisibility;
            set
            {
                toolBarVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public DisplayImageWindowViewModel()
        {
            ShowToolbarCommand = new RelayCommand(ShowToolbar);
            _aggregator.GetEvent<HideToolbarEvent>().Subscribe(Collapse);
        }

        private void Collapse()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                ToolBarVisibility = Visibility.Collapsed;
            }));
        }

        private void ShowToolbar(object obj)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                ToolBarVisibility = Visibility.Visible;
            }));
        }
    }
}