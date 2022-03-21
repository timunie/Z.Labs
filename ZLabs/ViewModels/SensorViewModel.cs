using System.Collections.ObjectModel;
using ZLabs.Models;

namespace ZLabs.ViewModels;

public class SensorViewModel : ViewModelBase, IPage
{
    private Sensor _sensor;

    public string Name => _sensor.Name;
    public string ImagePath => _sensor.ImagePath;
    public ObservableCollection<SensorSetting> Settings => _sensor.Settings;

    public SensorViewModel(Sensor sensor)
    {
        _sensor = sensor;
    }

    public static SensorViewModel SampleViewModel { get; }

    static SensorViewModel()
    {
        SampleViewModel = new SensorViewModel(new Sensor("Test", "/Assets/Img/Sensors/Pressure.png"));
    }
}