using NModbus;

namespace HardwareLib
{
    public class Device
    {
        private byte devAddress;
        private IModbusMaster modbus;

        public Device(byte DevAddr, IModbusMaster Modbus)
        {
            devAddress = DevAddr;
            modbus = Modbus;
        }

        #region общие
        /// <summary>
        /// Тип лаборатории
        /// </summary>
        public ushort CheckType
        {
            get
            {
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(devAddress, 1, 1);
                }
                return tmp[0];
            }
        }

        private void Read()
        {
            modbus.WriteSingleRegister(1, 30, 43220);
        }
        private void Write()
        {
            modbus.WriteSingleRegister(1, 30, 49374);
        }

        public ushort[] Coeffs
        {
            get
            {
                Read();
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(devAddress, 31, 17);
                }
                return tmp;
            }
        }
        public ushort[] Values
        {
            get
            {
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(devAddress, 9, 8);
                }
                return tmp;
            }
        }
        public ushort[] Accel
        {
            get
            {
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(devAddress, 25, 3);
                }
                return tmp;
            }
        }
        public ushort Volt_AKB
        {
            get
            {
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(1, 13, 1);
                }
                return tmp[0];
            }
        }
        public byte[] Firmware_Ver
        {
            get
            {
                ushort[] tmp;
                lock (modbus)
                {
                    tmp = modbus.ReadHoldingRegisters(1, 0, 1);
                }
                byte byte_1 = (byte)(tmp[0]);
                byte byte_2 = (byte)(tmp[0] >> 8);
                return new byte[] { byte_1, byte_2 };
            }
        }
        public void Dispose() { }
        public void WriteAccel_Range(int Range)
        {
            ushort val = 0;
            switch (Range)
            {
                //2g
                case 0:
                    val = 0;
                    break;
                //4g
                case 1:
                    val = 4;
                    break;
                //8g
                case 2:
                    val = 8;
                    break;
            }
            modbus.WriteMultipleRegisters(1, 3, new ushort[] { (ushort)val });
        }

        #endregion

        #region PHYS
        // коэффициенты заводские
        public struct PhysStaticCoef
        {
            public float k_t;
            public Int16 delta_t;
            public float k_pressure;
            public Int16 delta_pressure;
            public float k_voltage;
            public float delta_voltage;
            public float k_amper;
            public float delta_amper;
        }
        //поправочные коэффициенты
        public struct PhysChangeableCoeff
        {
            public float coeff_t;
            public float coeff_pressure;
            public float coeff_voltage;
            public float coeff_amper;
                
        }
        // константы датчиков
        public static class PhysConstant
        {
            public static float tempConst = 144.93F;
            public static float pressureConst = 357F;
            public static float voltageConst = 7.075F;
            public static float amperConst = 0.92F;
            public static float teslaConst = 66.67F;
            public static float g2Const = 0.064F;
            public static float g4Const = 0.128F;
            public static float g8Const = 0.256F;

        }

        //чтение пользовательских настроек
        public PhysChangeableCoeff ReadPhysUserCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 55, 4);
            PhysChangeableCoeff physChangeableCoeffs = new PhysChangeableCoeff();

            physChangeableCoeffs.coeff_t = (Int16)registers[0] * 0.01F;
            physChangeableCoeffs.coeff_pressure = (Int16)registers[1] * 0.01F;
            physChangeableCoeffs.coeff_voltage = (Int16)registers[2] * 0.01F;
            physChangeableCoeffs.coeff_amper = (Int16)registers[3] * 0.01F;

            return physChangeableCoeffs;
        }

        //чтение заводских настроек
        public PhysStaticCoef ReadPhysDefaultCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 40, 8);
            PhysStaticCoef physStaticCoeffs = new PhysStaticCoef();
            
            physStaticCoeffs.k_t = (Int16)registers[0] * 0.01F;
            physStaticCoeffs.delta_t = (Int16)registers[1];

            physStaticCoeffs.k_pressure = (Int16)registers[2] * 0.01F;
            physStaticCoeffs.delta_pressure = (Int16)registers[3];

            physStaticCoeffs.k_voltage = (Int16)registers[4] * 0.01F;
            physStaticCoeffs.delta_voltage = (Int16)registers[5] * 0.01F;

            physStaticCoeffs.k_amper = (Int16)registers[6] * 0.01F;
            physStaticCoeffs.delta_amper = (Int16)registers[7] * 0.01F;
            return physStaticCoeffs;
        }

        //Сброс пользовательских настроек: запись заводских настроек в пользовательские
        public void ResetPhysCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 4);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadPhys_UserCoeff_t()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 55, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WritePhys_UserCoeff_t(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 55, (ushort)regWrite);
            Write();
        }
        public void ResetPhys_Coeff_t()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 1);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadPhys_UserCoeff_pressure()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 56, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WritePhys_UserCoeff_pressure(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 56, (ushort)regWrite);
            Write();
        }
        public void ResetPhys_Coeff_pressure()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 49, 1);
            modbus.WriteMultipleRegisters(devAddress, 56, registers);
            Write();
        }


        public float ReadPhys_UserCoeff_voltage()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 57, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WritePhys_UserCoeff_voltage(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 57, (ushort)regWrite);
            Write();
        }
        public void ResetPhys_Coeff_voltage()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 50, 1);
            modbus.WriteMultipleRegisters(devAddress, 57, registers);
            Write();
        }


        public float ReadPhys_UserCoeff_amper()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 58, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WritePhys_UserCoeff_amper(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 58, (ushort)regWrite);
            Write();
        }
        public void ResetPhys_Coeff_amper()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 51, 1);
            modbus.WriteMultipleRegisters(devAddress, 58, registers);
            Write();
        }
        #endregion

        #region CHEM
        // коэффициенты заводские
        public struct ChemStaticCoef
        {
            public float k_thermocouple;
            public Int16 delta_thermocouple;
            public float k_pt;
            public Int16 delta_pt;
            public float k_ph_volt;
            public float delta_ph_volt;
            public float k_ph_ph;
            public float delta_ph_ph;
        }
        //поправочные коэффициенты
        public struct ChemChangeableCoeff
        {
            public float coeff_thermocouple;
            public float coeff_pt;
            public float coeff_ph_volt;
            public float coeff_ph_ph;

        }
        // константы датчиков
        public static class ChemConstant
        {
            public static float thermocoupleConst = 385F;
            public static float ptConst = 594.25F;
            public static float phConst = 0.323F;
        }

        //чтение пользовательских настроек
        public ChemChangeableCoeff ReadChemUserCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 55, 4);
            ChemChangeableCoeff chemChangeableCoeffs = new ChemChangeableCoeff();

            chemChangeableCoeffs.coeff_thermocouple = (Int16)registers[0] * 0.01F;
            chemChangeableCoeffs.coeff_pt = (Int16)registers[1] * 0.01F;
            chemChangeableCoeffs.coeff_ph_volt = (Int16)registers[2] * 0.01F;
            chemChangeableCoeffs.coeff_ph_ph = (Int16)registers[3] * 0.01F;

            return chemChangeableCoeffs;
        }

        //чтение заводских настроек
        public ChemStaticCoef ReadChemDefaultCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 40, 8);
            ChemStaticCoef chemStaticCoeffs = new ChemStaticCoef();

            chemStaticCoeffs.k_thermocouple = (Int16)registers[0] * 0.01F;
            chemStaticCoeffs.delta_thermocouple = (Int16)registers[1];

            chemStaticCoeffs.k_pt = (Int16)registers[2] * 0.01F;
            chemStaticCoeffs.delta_pt = (Int16)registers[3];

            chemStaticCoeffs.k_ph_volt = (Int16)registers[4] * 0.01F;
            chemStaticCoeffs.delta_ph_volt = (Int16)registers[5] * 0.01F;

            chemStaticCoeffs.k_ph_ph = (Int16)registers[6] * 0.01F;
            chemStaticCoeffs.delta_ph_ph = (Int16)registers[7] * 0.01F;
            return chemStaticCoeffs;
        }

        //Сброс пользовательских настроек: запись заводских настроек в пользовательские
        public void ResetChemCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 4);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadChem_UserCoeff_thermocouple()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 55, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteChem_UserCoeff_thermocouple(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 55, (ushort)regWrite);
            Write();
        }
        public void ResetChem_Coeff_thermocouple()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 1);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadChem_UserCoeff_pt()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 56, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteChem_UserCoeff_pt(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 56, (ushort)regWrite);
            Write();
        }
        public void ResetChem_Coeff_pt()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 49, 1);
            modbus.WriteMultipleRegisters(devAddress, 56, registers);
            Write();
        }

        public float ReadChem_UserCoeff_ph_volt()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 57, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteChem_UserCoeff_ph_volt(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 57, (ushort)regWrite);
            Write();
        }
        public void ResetChem_Coeff_ph_volt()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 50, 1);
            modbus.WriteMultipleRegisters(devAddress, 57, registers);
            Write();
        }

        public float ReadChem_UserCoeff_ph_ph()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 58, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteChem_UserCoeff_ph_ph(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 58, (ushort)regWrite);
            Write();
        }
        public void ResetChem_Coeff_ph_ph()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 51, 1);
            modbus.WriteMultipleRegisters(devAddress, 58, registers);
            Write();
        }
        #endregion

        #region BIO
        // коэффициенты заводские
        public struct BioStaticCoef
        {
            public float k_ptc;
            public Int16 delta_ptc;
            public float k_ph_volt;
            public float delta_ph_volt;
            public float k_ph_ph;
            public float delta_ph_ph;
        }
        //поправочные коэффициенты
        public struct BioChangeableCoeff
        {
            public float coeff_ptc;
            public float coeff_ph_volt;
            public float coeff_ph_ph;

        }
        // константы датчиков
        public static class BioConstant
        {
            public static float ptcConst = 144.93F;
            public static float phConst = 0.323F;
        }

        //чтение пользовательских настроек
        public BioChangeableCoeff ReadBioUserCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 55, 3);
            BioChangeableCoeff bioChangeableCoeffs = new BioChangeableCoeff();

            bioChangeableCoeffs.coeff_ptc = (Int16)registers[0] * 0.01F;
            bioChangeableCoeffs.coeff_ph_volt = (Int16)registers[1] * 0.01F;
            bioChangeableCoeffs.coeff_ph_ph = (Int16)registers[2] * 0.01F;

            return bioChangeableCoeffs;
        }

        //чтение заводских настроек
        public BioStaticCoef ReadBioDefaultCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 40, 6);
            BioStaticCoef bioStaticCoeffs = new BioStaticCoef();

            bioStaticCoeffs.k_ptc = (Int16)registers[0] * 0.01F;
            bioStaticCoeffs.delta_ptc = (Int16)registers[1];

            bioStaticCoeffs.k_ph_volt = (Int16)registers[2] * 0.01F;
            bioStaticCoeffs.delta_ph_volt = (Int16)registers[3] * 0.01F;

            bioStaticCoeffs.k_ph_ph = (Int16)registers[4] * 0.01F;
            bioStaticCoeffs.delta_ph_ph = (Int16)registers[5] * 0.01F;
            return bioStaticCoeffs;
        }

        //Сброс пользовательских настроек: запись заводских настроек в пользовательские
        public void ResetBioCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 46, 3);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadBio_UserCoeff_ptc()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 55, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteBio_UserCoeff_ptc(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 55, (ushort)regWrite);
            Write();
        }
        public void ResetBio_Coeff_ptc()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 46, 1);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadBio_UserCoeff_ph_volt()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 56, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteBio_UserCoeff_ph_volt(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 56, (ushort)regWrite);
            Write();
        }
        public void ResetBio_Coeff_ph_volt()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 47, 1);
            modbus.WriteMultipleRegisters(devAddress, 56, registers);
            Write();
        }

        public float ReadBio_UserCoeff_ph_ph()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 57, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteBio_UserCoeff_ph_ph(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 57, (ushort)regWrite);
            Write();
        }
        public void ResetBio_Coeff_ph_ph()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 1);
            modbus.WriteMultipleRegisters(devAddress, 57, registers);
            Write();
        }
        #endregion

        #region ECO
        // коэффициенты заводские
        public struct EcoStaticCoef
        {
            public float k_ptc;
            public Int16 delta_ptc;
            public float k_ph_volt;
            public float delta_ph_volt;
            public float k_ph_ph;
            public float delta_ph_ph;
            public float k_electrod;
            public Int16 delta_electrod;
        }
        //поправочные коэффициенты
        public struct EcoChangeableCoeff
        {
            public float coeff_ptc;
            public float coeff_ph_volt;
            public float coeff_ph_ph;
            public float coeff_electrod;

        }
        // константы датчиков
        public static class EcoConstant
        {
            public static float ptcConst = 144.93F;
            public static float phConst = 0.323F;
        }

        //чтение пользовательских настроек
        public EcoChangeableCoeff ReadEcoUserCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 55, 4);
            EcoChangeableCoeff ecoChangeableCoeffs = new EcoChangeableCoeff();

            ecoChangeableCoeffs.coeff_ptc = (Int16)registers[0] * 0.01F;
            ecoChangeableCoeffs.coeff_ph_volt = (Int16)registers[1] * 0.01F;
            ecoChangeableCoeffs.coeff_ph_ph = (Int16)registers[2] * 0.01F;
            ecoChangeableCoeffs.coeff_electrod = (Int16)registers[3] * 0.01F;

            return ecoChangeableCoeffs;
        }

        //чтение заводских настроек
        public EcoStaticCoef ReadEcoDefaultCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 40, 8);
            EcoStaticCoef ecoStaticCoeffs = new EcoStaticCoef();

            ecoStaticCoeffs.k_ptc = (Int16)registers[0] * 0.01F;
            ecoStaticCoeffs.delta_ptc = (Int16)registers[1];

            ecoStaticCoeffs.k_ph_volt = (Int16)registers[2] * 0.01F;
            ecoStaticCoeffs.delta_ph_volt = (Int16)registers[3] * 0.01F;

            ecoStaticCoeffs.k_ph_ph = (Int16)registers[4] * 0.01F;
            ecoStaticCoeffs.delta_ph_ph = (Int16)registers[5] * 0.01F;

            ecoStaticCoeffs.k_electrod = (Int16)registers[6] * 0.01F;
            ecoStaticCoeffs.delta_electrod = (Int16)registers[7];
            return ecoStaticCoeffs;
        }

        //Сброс пользовательских настроек: запись заводских настроек в пользовательские
        public void ResetEcoCoef()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 4);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadEco_UserCoeff_ptc()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 55, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteEco_UserCoeff_ptc(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 55, (ushort)regWrite);
            Write();
        }
        public void ResetEco_Coeff_ptc()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 48, 1);
            modbus.WriteMultipleRegisters(devAddress, 55, registers);
            Write();
        }

        public float ReadEco_UserCoeff_ph_volt()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 56, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteEco_UserCoeff_ph_volt(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 56, (ushort)regWrite);
            Write();
        }
        public void ResetEco_Coeff_ph_volt()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 49, 1);
            modbus.WriteMultipleRegisters(devAddress, 56, registers);
            Write();
        }


        public float ReadEco_UserCoeff_ph_ph()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 57, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteEco_UserCoeff_ph_ph(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 57, (ushort)regWrite);
            Write();
        }
        public void ResetEco_Coeff_ph_ph()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 50, 1);
            modbus.WriteMultipleRegisters(devAddress, 57, registers);
            Write();
        }

        public float ReadEco_UserCoeff_electrod()
        {
            Read();
            var regRead = modbus.ReadHoldingRegisters(devAddress, 58, 1);
            return (Int16)regRead[0] * 0.01F;
        }
        public void WriteEco_UserCoeff_electrod(float coeff)
        {
            var regWrite = Convert.ToInt16(coeff * 100);
            modbus.WriteSingleRegister(devAddress, 58, (ushort)regWrite);
            Write();
        }
        public void ResetEco_Coeff_electrod()
        {
            Read();
            var registers = modbus.ReadHoldingRegisters(devAddress, 51, 1);
            modbus.WriteMultipleRegisters(devAddress, 58, registers);
            Write();
        }
        #endregion

    }
}








