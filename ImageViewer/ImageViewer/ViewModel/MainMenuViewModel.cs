using ImageViewer.Methods;
using ImageViewer.Model;
using ImageViewer.Model.Event;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel
{
    public class MainMenuViewModel : BaseViewModel
    {

        public ObservableCollection<Image> ImageList;

        public RelayCommand DialogCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }

        public MainMenuViewModel()
        {
            ImageList = new ObservableCollection<Image>();
            DialogCommand = new RelayCommand(DialogExecute, DialogCanExecute);
            ClearCommand = new RelayCommand(ClearExecute, ClearCanExecute);
            _aggregator.GetEvent<SendImage>().Subscribe(item => {
                if (ImageList.Contains(item) == false)
                    ImageList.Add(item);
            });
        }

        private void ClearExecute(object obj)
        {
            _aggregator.GetEvent<ClearEvent>().Publish();
            ImageList.Clear();
        }
    

        private bool ClearCanExecute(object obj)
        {
            return true;
        }

        private void DialogExecute(object obj)
        {
            Task.Run(() => DialogMethod());
        }

        public void DialogMethod()
        {
            FileDialogMethod fdm = new FileDialogMethod();
            fdm.ReturnFilesFromDialog(ImageList);
            _aggregator.GetEvent<FileDialogEvent>().Publish(ImageList);
        }

        private bool DialogCanExecute(object obj)
        {
            return true;
        }
    }
}
