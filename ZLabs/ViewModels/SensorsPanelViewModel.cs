using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using ZLabs.Models;

namespace ZLabs.ViewModels;

public class SensorPanelViewModel : ViewModelBase, IPage
{
    public string Name => "Датчики";
    public string ImagePath => "/Assets/Img/Compass.png";
    
    
    private bool _isBluetoothTab;
    public bool IsBluetoothTab
    {
        get => _isBluetoothTab;
        set => this.RaiseAndSetIfChanged(ref _isBluetoothTab, value);
    }

    public ReactiveCommand<ConnectionType, Unit> ToggleBluetoothUsb;


    public SensorPanelViewModel()
    {
        ToggleBluetoothUsb = ReactiveCommand.Create<ConnectionType>(type =>
        {
            IsBluetoothTab = type == ConnectionType.Bluetooth;
        });
    }
    
}

public enum ConnectionType
{
    Usb,
    Bluetooth
}