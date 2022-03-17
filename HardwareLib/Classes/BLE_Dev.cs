using System.ComponentModel;

namespace HardwareLib.Classes
{
    public class BLE_Dev : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                RaisePropertyChanged(PropertyName);
        }
        public BLE_Dev() { }
        public short rssi { get; set; }
        public Device BleDevice { get; set; }
        
        public string DevName { get; set; }
        
    }
}
