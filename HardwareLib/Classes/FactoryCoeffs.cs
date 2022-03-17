using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib
{
    public class FactoryCoeffs
    {
        public FactoryCoeffs() { }
        public float k_Channel0 { get; set; }
        public float k_Channel1 { get; set; }
        public float k_Channel2 { get; set; }
        public float k_Channel3 { get; set; }
        public float delta_Channel0 { get; set; }
        public float delta_Channel1 { get; set; }
        public float delta_Channel2 { get; set; }
        public float delta_Channel3 { get; set; }
        public float delta_ToutSensor { get; set; }
        public float delta_HumiditySensor { get; set; }
        public float delta_LightSensor { get; set; }
        public float delta_AbsolutePressureSensor { get; set; }
        public float k_AmpermetrSensor { get; set; }
        public float delta_AmpermetrSensor { get; set; }
        public float k_VoltmeterSensor { get; set; }
        public float delta_VoltmeterSensor { get; set; }
    }
}
