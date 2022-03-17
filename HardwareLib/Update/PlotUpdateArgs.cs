using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class PlotUpdateArgs :EventArgs
    {
        public float Value { get; set; }
        public DateTime Time { get; set; }
        public PlotUpdateArgs(float Val)
        {
            this.Value = Val;  
            this.Time = DateTime.Now;
        }
    }
    public class Plot_PH_UpdateArgs : EventArgs
    {
        public float Value_Volt { get; set; }
        public float Value_PH { get; set; }
        public DateTime Time { get; set; }
        public Plot_PH_UpdateArgs(float Val_Volt, float Val_PH)
        {
            this.Value_Volt = Val_Volt;
            this.Value_PH = Value_PH;
            this.Time = DateTime.Now;
        }
    }
    public enum SensorName
    {
        TempSensor,
        PhSensor,
        ElectricalConductivitySensor,
        HumiditySensor,
        LightSensor,
        TempOutsideSensor,
        AbsolutePressureSensor,
        TeslametrSensor,
        VoltmeterSensor,
        AmpermetrSensor,
        AccelerometerXSensor,
        AccelerometerYSensor,
        AccelerometerZSensor,
        NitrateIonSensor,
        СhlorideIonSensor,
        BloodPressureSensor,
        PulseSensor,
        BodyTempSensor,
        RespiratoryRateSensor,
        SoundSensor,
        SoilMoistureSensor,
        MultiSensor,
        СarbonMonoxideSensor,
        ElectrocardiographSensor,
        WristForceSensor,
        ColorimeterSensor,

    }
    public enum PlotLineThickness
    {

    }
}
