using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Model.Event;

namespace ImageViewer.ViewModel.ImageWindowViewModels
{
    class RegionInformationViewModel : BaseViewModel
    {
        string _minValues;
        string _maxValues;
        string _averageValues;

        public string MinValues
        {
            get
            {
                return _minValues;
            }
            set
            {
                _minValues = value;
                NotifyPropertyChanged();
            }
        }
        public string MaxValues
        {
            get
            {
                return _maxValues;
            }
            set
            {
                _maxValues = value;
                NotifyPropertyChanged();
            }
        }
        public string AverageValues
        {
            get
            {
                return _averageValues;
            }
            set
            {
                _averageValues = value;
                NotifyPropertyChanged();
            }
        }
        public RegionInformationViewModel()
        {
            _aggregator.GetEvent<SendRegionInformationEvent>().Subscribe(SetPixelInformation);
        }

        private void SetPixelInformation(Dictionary<string,Object> parameters)
        {
            try
            {
                double[] averages = (double[])parameters["Averages"];
                byte[] mins = (byte[])parameters["Mins"];
                byte[] maxs = (byte[])parameters["Maxs"];
                AverageValues = $"Red:\t{Math.Round(averages[0], 2)}\nGreen:\t{Math.Round(averages[1], 2)}\nBlue:\t{Math.Round(averages[2], 2)}\nAlpha:\t{Math.Round(averages[3], 2)}";
                MinValues = $"Red:\t{mins[0]}\nGreen:\t{mins[1]}\nBlue:\t{mins[2]}\nAlpha:\t{mins[3]}";
                MaxValues = $"Red:\t{maxs[0]}\nGreen:\t{maxs[1]}\nBlue:\t{maxs[2]}\nAlpha:\t{maxs[3]}";
            }
            catch (KeyNotFoundException)
            {

                return;
            }
        }
    }
}
