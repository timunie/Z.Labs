using System;
using System.ComponentModel;
using System.Windows;

namespace HardwareLib
{
    public class Data: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                RaisePropertyChanged(PropertyName);
        }
        public DateTime Time { get; set; }
        public Data()
        {
            Time = DateTime.Now;
        }

        public float TempSensor { get; set; }
        public float TempSensor_Calibr { get; set; }

        #region pHSensor
        public float PhSensor_inPh { get; set; } //Ph
        public float PhSensor_inPh_Calibr { get; set; } //Ph
        public float PhSensor_inVolt { get; set; } //Ph  в вольтах
        public float PhSensor_Display { get; set; } //чисто вывод на экран
        public string PhUnit_Display { get; set; } //чисто вывод на экран ед. измер
        #endregion

        public float ElectricalConductivitySensor { get; set; }
        public float ElectricalConductivitySensor_Calibr { get; set; }

        public float HumiditySensor { get; set; }
        public float HumiditySensor_Calibr { get; set; }

        public float LightSensor { get; set; }
        public float LightSensor_Calibr { get; set; }

        public float TempOutsideSensor { get; set; }
        public float TempOutsideSensor_Calibr { get; set; }
        public float AbsolutePressureSensor { get; set; }
        public float AbsolutePressureSensor_Calibr { get; set; }
        public float TeslametrSensor { get; set; }
        public float TeslametrSensor_Calibr { get; set; }
        public float VoltmeterSensor { get; set; }
        public float VoltmeterSensor_Calibr { get; set; }
        public float AmpermetrSensor { get; set; }
        public float AmpermetrSensor_Calibr { get; set; }
        public float AccelerometerXSensor { get; set; }
        public float AccelerometerYSensor { get; set; }
        public float AccelerometerZSensor { get; set; }
        public float NitrateIonSensor { get; set; }
        public float СhlorideIonSensor { get; set; }
        public float BloodPressureSensor { get; set; }
        public float PulseSensor { get; set; }
        public float BodyTempSensor { get; set; }
        public float RespiratoryRateSensor { get; set; }
        public float SoundSensor { get; set; }
        public float SoilMoistureSensor { get; set; }
        public float MultiSensor { get; set; }
        public float СarbonMonoxideSensor { get; set; }

        public float ElectrocardiographSensor { get; set; }
        public float WristForceSensor { get; set; }
        public float ColorimeterSensor { get; set; }

        public bool[] Visibility { get; set; }

    }
}
