using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Platform;
using DynamicData.Binding;
using HardwareLib.Classes;
using ReactiveUI;
using ZLabs.Models;

namespace ZLabs.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IPage _selectedPage;

        public ObservableCollection<Sensor> Sensors { get; }

        public IPage SelectedPage
        {
            get => _selectedPage;
            set => this.RaiseAndSetIfChanged(ref _selectedPage, value);
        }

        public MainWindowViewModel()
        {
            var sensors = Enum.GetValues(typeof(SensorType)).Cast<SensorType>().Select(type =>
            {
                return SensorFabric.GetSensor(type);
            });
            Sensors = new ObservableCollection<Sensor>(sensors);
        }
    }
}