using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HardwareLib.Board_BLE;

namespace HardwareLib.Update
{
    public class DataBoard : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                RaisePropertyChanged(PropertyName);
        }

        public DataBoard() { }

        public bool Press_ok { get; set; }
        public bool Hum_ok { get; set; }
        public bool Light_ok { get; set; }
        public bool Accel_ok { get; set; }
        public Service.ConductScale ConductScale { get; set; }

        public float TempADC { get; set; }
        public float Channel0 { get; set; }
        public float Channel1 { get; set; }
        public float Channel2 { get; set; }
        public float Channel3 { get; set; }
        public Int16 AccelX { get; set; }
        public Int16 AccelY { get; set; }
        public Int16 AccelZ { get; set; }
        public Service.AccelScale AccelScale { get; set; }
        public float Light { get; set; }
        public float Humidity { get; set; }
        public float Pressure { get; set; }

    }
}
