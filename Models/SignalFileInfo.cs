using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalAnalayzer.Models
{
    public partial class SignalFileInfo : ObservableObject
    {
        [ObservableProperty]
        private string fileName;
        [ObservableProperty]
        private string signalName;
        [ObservableProperty]
        private double samplingFreq;
        [ObservableProperty]
        private double durationSec;
        [ObservableProperty]
        private double min;
        [ObservableProperty]
        private double max;
        [ObservableProperty]
        private double expectMate;
    }
}
