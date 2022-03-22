using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData.Tests;
using ZLabs.Models;

namespace ZLabs.ViewModels;

public class SensorsSetViewModel : ViewModelBase, IPage
{
    private ObservableCollection<Sensor> Sensors { get; set;  }


    public string Name { get; } = "Связка датчиков";
    public string ImagePath { get; } = "/Assets/Img/Pack.png";

    public SensorsSetViewModel(IEnumerable<Sensor> sensors)
    {
        Sensors = new ObservableCollection<Sensor>(sensors);
    }

    public SensorsSetViewModel()
    {
        Sensors = new ObservableCollection<Sensor>()
        {
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
            new("Test", "/Assets/Img/Calibr.png"),
        };
    }

}