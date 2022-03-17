using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class UserCoeff_Bio
    {
        public UserCoeff_Bio() { }
        public ushort step_TempSensor { get; set; }
        public float delta_TempSensor { get; set; }
        public float k_TempSensor { get; set; }

        public ushort step_pHSensor_inPH { get; set; }
        public float delta_pHSensor_inPH { get; set; }
        public float k_pHSensor_inPH { get; set; }
        public float delta_ToutSensor { get; set; }
        public float delta_HumiditySensor { get; set; }
        public float delta_LightSensor { get; set; }
    }
    public class UserCalibrVal_BIO
    {
        public float x1_tempSensor; //текущее значение 
        public float x2_tempSensor; //текущее значение 
        public float y1_tempSensor; //фактическое значение 
        public float y2_tempSensor; //фактическое значение 

        public float x1_pHSensor; //текущее значение 
        public float x2_pHSensor; //текущее значение 
        public float y1_pHSensor; //фактическое значение 
        public float y2_pHSensor; //фактическое значение 

        public float x1_ToutSensor; //текущее значение 
        public float y1_ToutSensor; //фактическое значение 

        public float x1_HumiditySensor; //текущее значение 
        public float y1_HumiditySensor; //фактическое значение 

        public float x1_LightSensor; //текущее значение 
        public float y1_LightSensor; //фактическое значение 
    }


    public class UserCoeff_Phys
    {
        public UserCoeff_Phys() { }
        public ushort step_TempSensor { get; set; }
        public float delta_TempSensor { get; set; }
        public float k_TempSensor { get; set; }

        public float delta_AbsolutePressure { get; set; }

        public ushort step_TeslametrSensor { get; set; }
        public float delta_TeslametrSensor { get; set; }
        public float k_TeslametrSensor { get; set; }

        public ushort step_VoltmeterSensor { get; set; }
        public float delta_VoltmeterSensor { get; set; }
        public float k_VoltmeterSensor { get; set; }

        public ushort step_AmpermetrSensor { get; set; }
        public float delta_AmpermetrSensor { get; set; }
        public float k_AmpermetrSensor { get; set; }
    }
    public class UserCalibrVal_Phys
    {
        public float x1_tempSensor; //текущее значение 
        public float x2_tempSensor; //текущее значение 
        public float y1_tempSensor; //фактическое значение 
        public float y2_tempSensor; //фактическое значение 

        public float x1_TeslametrSensor; //текущее значение 
        public float x2_TeslametrSensor; //текущее значение 
        public float y1_TeslametrSensor; //фактическое значение 
        public float y2_TeslametrSensor; //фактическое значение 

        public float x1_VoltmeterSensor; //текущее значение 
        public float x2_VoltmeterSensor; //текущее значение 
        public float y1_VoltmeterSensor; //фактическое значение 
        public float y2_VoltmeterSensor; //фактическое значение 

        public float x1_AmpermetrSensor; //текущее значение 
        public float x2_AmpermetrSensor; //текущее значение 
        public float y1_AmpermetrSensor; //фактическое значение 
        public float y2_AmpermetrSensor; //фактическое значение 

        public float x1_AbsolutePressure; //текущее значение  
        public float y1_AbsolutePressure; //фактическое значение 
    }

    public class UserCoeff_Chem
    {
        public UserCoeff_Chem() { }
        public ushort step_TempSensor { get; set; }
        public float delta_TempSensor { get; set; }
        public float k_TempSensor { get; set; }

        public ushort step_pHSensor_inPH { get; set; }
        public float delta_pHSensor_inPH { get; set; }
        public float k_pHSensor_inPH { get; set; }

        public ushort step_ElectricalConductivitySensor { get; set; }
        public float delta_ElectricalConductivitySensor { get; set; }
        public float k_ElectricalConductivitySensor { get; set; }
    }
    public class UserCalibrVal_Chem
    {
        public float x1_tempSensor; //текущее значение 
        public float x2_tempSensor; //текущее значение 
        public float y1_tempSensor; //фактическое значение 
        public float y2_tempSensor; //фактическое значение 

        public float x1_pHSensor; //текущее значение 
        public float x2_pHSensor; //текущее значение 
        public float y1_pHSensor; //фактическое значение 
        public float y2_pHSensor; //фактическое значение 

        public float x1_ElectricalConductivitySensor; //текущее значение 
        public float x2_ElectricalConductivitySensor; //текущее значение 
        public float y1_ElectricalConductivitySensor; //фактическое значение 
        public float y2_ElectricalConductivitySensor; //фактическое значение
    }
}


