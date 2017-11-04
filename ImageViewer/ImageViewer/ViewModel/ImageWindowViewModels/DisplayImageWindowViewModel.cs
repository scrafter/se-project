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
        public Visibility ToolbarVisibility
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
            _aggregator.GetEvent<HideToolbarEvent>().Subscribe(HideToolbar);
        }

        private void HideToolbar()
        {
            ToolbarVisibility = Visibility.Collapsed;
        }
        private void ShowToolbar(object obj)
        {
            ToolbarVisibility = Visibility.Visible;
        }
    }
}