using ImageViewer.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageViewer.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public RelayCommand AboutCommand { get; set; }
        public MainWindowViewModel()
        {
            AboutCommand = new RelayCommand(AboutExecute, AboutCanExecute);
        }

        private void AboutExecute(object obj)
        {
            MessageBox.Show("Authors: ehehehee", "Authors", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool AboutCanExecute(object obj)
        {
            return true;
        }
    }
}
