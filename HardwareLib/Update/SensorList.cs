using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HardwareLib.Classes
{
    public class SensorList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                RaisePropertyChanged(PropertyName);
        }
        public bool[] Visibility { get; set; }
    }
    public enum LabType
    {
        none = 0,
        phys = 1,
        chem = 2,
        eco = 3,
        bio = 4
    }

    //данный лист заполняется согласно ListBox Sensors_select
    public enum SensorType
    {
        TempSensor = 0,
        AbsolutePressureSensor = 1,
        TeslametrSensor = 2,
        VoltmeterSensor = 3,
        AmpermetrSensor = 4,
        AccelerometerXSensor = 5,
        AccelerometerYSensor = 6,
        AccelerometerZSensor = 7,
        ElectricalConductivitySensor = 8,
        HumiditySensor = 9,
        LightSensor = 10,
        TempOutsideSensor = 11,
        ColorimeterSensor = 12,
        PhSensor = 13
    }
    public enum PhSensor_Mode
    {
        volt = 0,
        ph = 1
    }
}
