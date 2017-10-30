﻿using ImageViewer.Model.Event;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageViewer.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private IEventAggregator _aggregator = GlobalEvent.GetEventAggregator();
        public MainView()
        {
            InitializeComponent();
            _aggregator.GetEvent<CollapseEvent>().Subscribe(Collapse);
        }

        private void Collapse()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.fileExplorerView.Visibility = Visibility.Hidden;
            }));
        }
    }
}
