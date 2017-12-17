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
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Prism.Events;
using System.Diagnostics;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    public class DisplayImageWindowViewModel : BaseViewModel, IDisposable
    {

        #region currentViewModels
        private Dictionary<Type, SubscriptionToken> _subscriptionTokens = new Dictionary<Type, SubscriptionToken>();
        private bool disposedValue = false;
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
        private static int _imageCounter = 0;
        private int _maxWindows=0;
        private List<PixelInformationView> _pivList = new List<PixelInformationView>();
        public RelayCommand ClosePIVsCommand { get; set; }
        public RelayCommand ShowToolbarCommand { get; set; }
        public RelayCommand DesynchronizationCommand { get; set; }
        private GridStatusEvent.GridStatus _gridStatus;
        public static Tools Tool = Tools.None;
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
        public static int ImageCounter
        {
            get
            {
                return _imageCounter;
            }
        }
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
        private ObservableCollection<Image> _imageList;

        public DisplayImageWindowViewModel()
        {
            GridStatus = GridStatusEvent.GridStatus.OneToOne;
            ShowToolbarCommand = new RelayCommand(ShowToolbar, x =>
            {
                return !disposedValue;
            });
            ClosePIVsCommand = new RelayCommand(ClosePIVs, x =>
            {
                return !disposedValue;
            });
            DesynchronizationCommand = new RelayCommand(Desynchronize, x =>
            {
                return !disposedValue;
            });
            SubscriptionToken subscriptionToken;
            subscriptionToken = _aggregator.GetEvent<HideToolbarEvent>().Subscribe(HideToolbar);
            _subscriptionTokens.Add(typeof(HideToolbarEvent), subscriptionToken);
            subscriptionToken = _aggregator.GetEvent<DisposeEvent>().Subscribe(Dispose);
            _subscriptionTokens.Add(typeof(DisposeEvent), subscriptionToken);
            subscriptionToken = _aggregator.GetEvent<SendPixelInformationViewEvent>().Subscribe(item => { _pivList.Add(item); });
            _subscriptionTokens.Add(typeof(SendPixelInformationViewEvent), subscriptionToken);
            subscriptionToken = _aggregator.GetEvent<DisplayImage>().Subscribe(item =>
            {
                _imageList = item;
                _imageCounter++;
                CreateMultiView(_imageList);
            });
            _subscriptionTokens.Add(typeof(DisplayImage), subscriptionToken);
            
            subscriptionToken = _aggregator.GetEvent<GridStatusEvent>().Subscribe((item) =>
            {
                GridStatus = item;
            });
            _subscriptionTokens.Add(typeof(GridStatusEvent), subscriptionToken);
        }

        private void Desynchronize(Object arg)
        {
            if (disposedValue)
                return;
            int id = Int16.Parse((string)arg);
            _aggregator.GetEvent<SynchronizationEvent>().Publish(id);
        }
        private void CreateMultiView(ObservableCollection<Image> _imageList)
        {
            if (disposedValue)
                return;
            switch (GridStatus)
            {
                case GridStatusEvent.GridStatus.OneToOne:
                    _maxWindows = 1;
                    GetImagePresentersFor1x1();
                    break;
                case GridStatusEvent.GridStatus.OneToTwo:
                    _maxWindows = 2;
                    GetImagePresentersFor1x2();
                    break;
                case GridStatusEvent.GridStatus.TwoToTwo:
                    _maxWindows = 4;
                    GetImagePresentersFor2x2();
                    break;
                case GridStatusEvent.GridStatus.ThreeToThree:
                    _maxWindows = 9;
                    GetImagePresentersFor3x3();
                    break;
            }
        }

        private void GetImagePresentersFor3x3()
        {
            if (disposedValue)
                return;
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1, Tool, _maxWindows);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2, Tool, _maxWindows);
                    break;
                case 3:
                    CurrentViewModel3 = new ImagePresenterViewModel(_imageList, 3, Tool,_maxWindows);
                    break;
                case 4:
                    CurrentViewModel4 = new ImagePresenterViewModel(_imageList, 4, Tool, _maxWindows);
                    break;
                case 5:
                    CurrentViewModel5 = new ImagePresenterViewModel(_imageList, 5, Tool, _maxWindows);
                    break;
                case 6:
                    CurrentViewModel6 = new ImagePresenterViewModel(_imageList, 6, Tool, _maxWindows);
                    break;
                case 7:
                    CurrentViewModel7 = new ImagePresenterViewModel(_imageList, 7, Tool, _maxWindows);
                    break;
                case 8:
                    CurrentViewModel8 = new ImagePresenterViewModel(_imageList, 8, Tool, _maxWindows);
                    break;
                case 9:
                    CurrentViewModel9 = new ImagePresenterViewModel(_imageList, 9, Tool, _maxWindows);
                    break;
                default:
                    _imageCounter = 1;
                    GetImagePresentersFor3x3();
                    break;
            }
        }

        private void GetImagePresentersFor2x2()
        {
            if (disposedValue)
                return;
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1, Tool, _maxWindows);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2, Tool, _maxWindows);
                    break;
                case 3:
                    CurrentViewModel3 = new ImagePresenterViewModel(_imageList, 3, Tool, _maxWindows);
                    break;
                case 4:
                    CurrentViewModel4 = new ImagePresenterViewModel(_imageList, 4, Tool, _maxWindows);
                    break;
                default:
                    _imageCounter = 1;
                    GetImagePresentersFor2x2();
                    break;
            }         
        }

        private void GetImagePresentersFor1x2()
        {
            if (disposedValue)
                return;
            switch (_imageCounter)
            {
                case 1:
                    CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1, Tool, _maxWindows);
                    break;
                case 2:
                    CurrentViewModel2 = new ImagePresenterViewModel(_imageList, 2, Tool, _maxWindows);
                    break;
                default:
                    _imageCounter = 1;
                    GetImagePresentersFor1x2();
                    break;
            }
        }

        private void GetImagePresentersFor1x1()
        {
            if (disposedValue)
                return;
            CurrentViewModel1 = new ImagePresenterViewModel(_imageList, 1, Tool,_maxWindows);
            _imageCounter = 1;
        }

        private void ClearImagePresenterList(int startingIndex)
        {
            
        }
        public void ClosePIVs(object obj)
        {
            if (disposedValue)
                return;
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
            if (disposedValue)
                return;
            ToolbarVisibility = Visibility.Collapsed;
        }
        private void ShowToolbar(object obj)
        {
            if (disposedValue)
                return;
            ToolbarVisibility = Visibility.Visible;
        }
    
        private void ClearImagePresenter()
        {
            if (disposedValue)
                return;
            CurrentViewModel5 = null;
            CurrentViewModel6 = null;
            CurrentViewModel7 = null;
            CurrentViewModel8 = null;
            CurrentViewModel9 = null;
        }

        #region IDisposable Support
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    handle.Dispose();
                }
                disposedValue = true;
            }
        }

         ~DisplayImageWindowViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            _currentViewModel1 = null;
            _currentViewModel2 = null;
            _currentViewModel3 = null;
            _currentViewModel4 = null;
            _currentViewModel5 = null;
            _currentViewModel6 = null;
            _currentViewModel7 = null;
            _currentViewModel8 = null;
            _currentViewModel9 = null;

            _aggregator.GetEvent<HideToolbarEvent>().Unsubscribe(_subscriptionTokens[typeof(HideToolbarEvent)]);
            _subscriptionTokens.Remove(typeof(HideToolbarEvent));
            _aggregator.GetEvent<DisposeEvent>().Unsubscribe(_subscriptionTokens[typeof(DisposeEvent)]);
            _subscriptionTokens.Remove(typeof(DisposeEvent));
            _aggregator.GetEvent<SendPixelInformationViewEvent>().Unsubscribe(_subscriptionTokens[typeof(SendPixelInformationViewEvent)]);
            _subscriptionTokens.Remove(typeof(SendPixelInformationViewEvent));
            _aggregator.GetEvent<DisplayImage>().Unsubscribe(_subscriptionTokens[typeof(DisplayImage)]);
            _subscriptionTokens.Remove(typeof(DisplayImage));
            _aggregator.GetEvent<GridStatusEvent>().Unsubscribe(_subscriptionTokens[typeof(GridStatusEvent)]);
            _subscriptionTokens.Remove(typeof(GridStatusEvent));

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}