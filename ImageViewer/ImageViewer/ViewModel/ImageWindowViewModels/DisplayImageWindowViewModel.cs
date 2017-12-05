using ImageViewer.Methods;
using ImageViewer.Model.Event;
using ImageViewer.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageViewer.Model;
using ImageViewer.View.ImagesWindow;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class DisplayImageWindowViewModel : BaseViewModel
    {
        private List<PixelInformationView> _pivList = new List<PixelInformationView>();
        public RelayCommand ClosePIVsCommand { get; set; }
        public RelayCommand ShowToolbarCommand { get; set; }
        private Visibility toolBarVisibility;
        public Visibility ToolbarVisibility
        {
            get
            {
                return toolBarVisibility;
            }
            set
            {
                toolBarVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public DisplayImageWindowViewModel()
        {
            ShowToolbarCommand = new RelayCommand(ShowToolbar);
            ClosePIVsCommand = new RelayCommand(ClosePIVs);
            _aggregator.GetEvent<HideToolbarEvent>().Subscribe(HideToolbar);
            _aggregator.GetEvent<SendPixelInformationViewEvent>().Subscribe(item => { _pivList.Add(item); });
        }

        public void ClosePIVs(object obj)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var item in _pivList)
                {
                    item.Close();
                }
            }));
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