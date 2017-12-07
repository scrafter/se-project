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
using System.Collections.ObjectModel;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class DisplayImageWindowViewModel : BaseViewModel
    {

        #region currentViewModels
        private BaseViewModel _currentViewModel1;
        private BaseViewModel _currentViewModel2;
        private BaseViewModel _currentViewModel3;
        private BaseViewModel _currentViewModel4;
        private BaseViewModel _currentViewModel5;
        private BaseViewModel _currentViewModel6;
        private BaseViewModel _currentViewModel7;
        private BaseViewModel _currentViewModel8;
        private BaseViewModel _currentViewModel9;

        public BaseViewModel CurrentViewModel1
        {
            get { return _currentViewModel1; }
            set
            {
                _currentViewModel1 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel2
        {
            get { return _currentViewModel2; }
            set
            {
                _currentViewModel2 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel3
        {
            get { return _currentViewModel3; }
            set
            {
                _currentViewModel3 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel4
        {
            get { return _currentViewModel4; }
            set
            {
                _currentViewModel4 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel5
        {
            get { return _currentViewModel5; }
            set
            {
                _currentViewModel5 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel6
        {
            get { return _currentViewModel6; }
            set
            {
                _currentViewModel6 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel7
        {
            get { return _currentViewModel7; }
            set
            {
                _currentViewModel7 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel8
        {
            get { return _currentViewModel8; }
            set
            {
                _currentViewModel8 = value;
                NotifyPropertyChanged();
            }
        }
        public BaseViewModel CurrentViewModel9
        {
            get { return _currentViewModel9; }
            set
            {
                _currentViewModel9 = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        private List<BaseViewModel> _imagePresenterList;
        private int _imageCounter = 0;
        private List<PixelInformationView> _pivList = new List<PixelInformationView>();
        public RelayCommand ClosePIVsCommand { get; set; }
        public RelayCommand ShowToolbarCommand { get; set; }
        private GridStatusEvent.GridStatus _gridStatus;
        public GridStatusEvent.GridStatus GridStatus
        {
            get { return _gridStatus; }
            set
            {
                _gridStatus = value;
                NotifyPropertyChanged();
                if(_gridStatus == GridStatusEvent.GridStatus.OneToOne)
                {
                    CurrentViewModel2 = null;
                    CurrentViewModel3 = null;
                    CurrentViewModel4 = null;
                    ClearImagePresenter();
                    if (_imageCounter > 1)
                        _imageCounter = 1;

                }
                if (_gridStatus == GridStatusEvent.GridStatus.OneToTwo)
                {
                    CurrentViewModel3 = null;
                    CurrentViewModel4 = null;
                    ClearImagePresenter();
                    if (_imageCounter > 2)
                        _imageCounter = 2;

                }
                if (_gridStatus == GridStatusEvent.GridStatus.TwoToTwo)
                {
                    ClearImagePresenter();
                    if (_imageCounter > 4)
                        _imageCounter = 4;
                }
            }
        }
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
        private ObservableCollection<Image> _imageList;

        public DisplayImageWindowViewModel()
        {
            ShowToolbarCommand = new RelayCommand(ShowToolbar);
            ClosePIVsCommand = new RelayCommand(ClosePIVs);
            _aggregator.GetEvent<HideToolbarEvent>().Subscribe(HideToolbar);
            _aggregator.GetEvent<SendPixelInformationViewEvent>().Subscribe(item => { _pivList.Add(item); });
            _aggregator.GetEvent<DisplayImage>().Subscribe(item =>
            {
                _imageList = item;
                _imageCounter++;
                CreateMultiView(_imageList);
            });
            GridStatus = GridStatusEvent.GridStatus.OneToOne;
            _aggregator.GetEvent<GridStatusEvent>().Subscribe((item) =>
            {
                GridStatus = item;
            });
        }

        private void CreateMultiView(ObservableCollection<Image> _imageList)
        {
            switch (GridStatus)
            {
                case GridStatusEvent.GridStatus.OneToOne:
                    GetImagePresentersFor1x1();
                    break;
                case GridStatusEvent.GridStatus.OneToTwo:
                    GetImagePresentersFor1x2();
                    break;
                case GridStatusEvent.GridStatus.TwoToTwo:
                    GetImagePresentersFor2x2();
                    break;
                case GridStatusEvent.GridStatus.ThreeToThree:
                    GetImagePresentersFor3x3();
                    break;
            }
        }

        private void GetImagePresentersFor3x3()
        {
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2);
                    break;
                case 3:
                    CurrentViewModel3 = new ImagePresenterViewModel(_imageList, 3);
                    break;
                case 4:
                    CurrentViewModel4 = new ImagePresenterViewModel(_imageList, 4);
                    break;
                case 5:
                    CurrentViewModel5 = new ImagePresenterViewModel(_imageList, 5);
                    break;
                case 6:
                    CurrentViewModel6 = new ImagePresenterViewModel(_imageList, 6);
                    break;
                case 7:
                    CurrentViewModel7 = new ImagePresenterViewModel(_imageList, 7);
                    break;
                case 8:
                    CurrentViewModel8 = new ImagePresenterViewModel(_imageList, 8);
                    break;
                case 9:
                    CurrentViewModel9 = new ImagePresenterViewModel(_imageList, 9);
                    break;
                default:
                    _imageCounter = 1;
                    CreateMultiView(_imageList);
                    break;
            }
        }

        private void GetImagePresentersFor2x2()
        {
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2);
                    break;
                case 3:
                    CurrentViewModel3 = new ImagePresenterViewModel(_imageList, 3);
                    break;
                case 4:
                    CurrentViewModel4 = new ImagePresenterViewModel(_imageList, 4);
                    break;
                default:
                    _imageCounter = 1;
                    CreateMultiView(_imageList);
                    break;
            }         
        }

        private void GetImagePresentersFor1x2()
        {
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2);
                    break;
                default:
                    _imageCounter = 1;
                    CreateMultiView(_imageList);
                    break;
            }
        }

        private void GetImagePresentersFor1x1()
        {
            CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1);
            _imageCounter = 0;
        }

        private void ClearImagePresenterList(int startingIndex)
        {
            
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
    
        private void ClearImagePresenter()
        {
            CurrentViewModel5 = null;
            CurrentViewModel6 = null;
            CurrentViewModel7 = null;
            CurrentViewModel8 = null;
            CurrentViewModel9 = null;
        }
    }
}