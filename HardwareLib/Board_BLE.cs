using HardwareLib.ModbusCRC16;
using HardwareLib.Update;

namespace HardwareLib
{
    public class Board_BLE
    {
        //подключение - в сервисе?
        //отключение - в сервисе?
        //попытки перечтения - в сервисе наверное
        //проверить то ли устройство вообще подключилось
        //время на ответ - 100 мкс
        private event EventHandler<bool> ErrorUpdate;

        public event EventHandler<DataBoardUpdateArgs> DataBoardUpdate;

        const byte unsupportMsg = 0xFF;
        const byte startByte = 0xAA;

        const byte data_exchange = 0x10;
        const byte get_HW_Config = 0x31;
        const byte get_UserData = 0x41;
        const byte set_UserData = 0x40;
        const byte proto_msgType_getInfo = 0x00;

        public enum Command
        {
            data_exchange = 0x10,
            get_HW_Config = 0x31,
            get_UserData = 0x41,
            set_UserData = 0x40,
            proto_msgType_getInfo = 0x00
        }

        //AccelScale currentAccelScale;
        //ConductScale currentConductScale;

        byte currentCommand;

        const int totalNumberOfAnalogChannel = 4;
        const byte user_calibrationData_Size = 68;

        public Info info;

        //public enum ConductScale
        //{
        //    k0 = 0,
        //    k1 = 1,
        //    k10 = 2,
        //    k100 = 3
        //}
        //public enum AccelScale
        //{
        //    g = 0,
        //    g2 = 1,
        //    g4 = 2,
        //    g8 = 3,
        //    g16 = 4
        //}
        public struct Info
        {
            public byte currentMode;
            public byte fwConfigState;
            public byte mainConfigState;
            public byte archiveMode;
            public byte mainProgSizePages;
            public byte atMode;
            public byte devType;
            public byte progErrors;
            public float bootloaderVer;
        }

        public Board_BLE ()
        {
            
        }

        #region
        private static byte[] CalculateCrc16Modbus(byte[] bytes)
        {
            byte[] myArray = new byte[bytes.Length - 2];
            for (int i = 0; i < myArray.Length; i++)
            {
                myArray[i] = bytes[i];
            }
            CrcStdParams.StandartParameters.TryGetValue(CrcAlgorithms.Crc16Modbus, out Parameters crc_p);
            Crc crc = new Crc(crc_p);
            crc.Initialize();
            var crc_bytes = crc.ComputeHash(myArray);
            return crc_bytes;
        }

        private bool[] ByteToBits (byte bt)
        {
            bool[] bits = new bool[8];
            for (int i=0; i<8; i++)
            {
                bits[i] = (bt & (1 << i)) != 0;
            }
            return bits;
        }

        private bool CheckAnswer(byte[] answerArray)
        {
            bool correct;
            var crc = CalculateCrc16Modbus(answerArray);
            correct = crc[0] == answerArray[answerArray.Length - 2] && crc[1] == answerArray[answerArray.Length - 1];
            return correct;
        }
        #endregion

        public byte GetCurrentCommand() { return currentCommand; }

        public byte[] Proto_msgType_getInfo()
        {
            currentCommand = proto_msgType_getInfo;

            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;

            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];

            writeMessage[3] = proto_msgType_getInfo;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        }
        public Info Answer_Proto_msgType_getInfo(byte[] buffer)
        {
            info = new Info();
            if (buffer[0] == startByte)
            {
                var length = new byte[2] { buffer[1], buffer[2] };

                if (buffer.Length == BitConverter.ToInt16(length, 0))
                {
                    if (buffer[3] == proto_msgType_getInfo && CheckAnswer(buffer))
                    {
                        info.currentMode = buffer[4];
                        info.fwConfigState = buffer[5];
                        info.mainConfigState = buffer[6];
                        info.archiveMode = buffer[7];
                        info.mainProgSizePages = buffer[8];
                        info.atMode = buffer[9];
                        info.devType = buffer[10];
                        info.progErrors = buffer[11];
                        ushort low = (ushort)(Convert.ToUInt16(buffer[12]) & 0xFF);
                        ushort hight = (ushort)(Convert.ToUInt16(buffer[12]) >> 8);

                        
                    }
                    else
                    {
                        ErrorUpdate?.Invoke(this, true);
                    }
                }
                else
                {
                    ErrorUpdate?.Invoke(this, true);
                }
            }
            return info;
        }
        
        
        public byte[] DataExchange_Read()
        {
            currentCommand = data_exchange;
           
            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = data_exchange;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        }
        public void Answer_DataExchange_Read(byte[] buffer)
        {
            if (buffer[0] == startByte)
            {
                var length = new byte[2] { buffer[1], buffer[2] };
                if (buffer.Length == BitConverter.ToInt16(length, 0))
                {
                    if (buffer[3] == data_exchange && CheckAnswer(buffer))
                    {
                        //получаем сырые данные
                        DataBoard dataBoard = new DataBoard();
                        
                        var state = ByteToBits(buffer[4]);
                        dataBoard.Press_ok = state[3];
                        dataBoard.Hum_ok = state[2];
                        dataBoard.Light_ok = state[1];
                        dataBoard.Accel_ok = state[0];

                        //dataBoard.ConductScale = (ConductScale)buffer[5];

                        //dataBoard.TempADC = BitConverter.ToInt16(buffer, 10) * 0.01F;

                        //dataBoard.Channel0 = BitConverter.ToSingle(buffer, 12);
                        //dataBoard.Channel1 = BitConverter.ToSingle(buffer, 16);
                        //dataBoard.Channel2 = BitConverter.ToSingle(buffer, 20);
                        //dataBoard.Channel3 = BitConverter.ToSingle(buffer, 24);

                        //dataBoard.AccelX = BitConverter.ToInt16(buffer, 28);
                        //dataBoard.AccelY = BitConverter.ToInt16(buffer, 30);
                        //dataBoard.AccelZ = BitConverter.ToInt16(buffer, 32);
                        //dataBoard.AccelScale = (AccelScale)buffer[34];

                        dataBoard.Light = BitConverter.ToSingle(buffer, 35);
                        dataBoard.Humidity = BitConverter.ToUInt16(buffer, 39) * 0.01F;

                        dataBoard.Pressure = BitConverter.ToSingle(buffer, 43);

                    }
                    else
                    {
                        ErrorUpdate?.Invoke(this, true);
                    }
                }
                else
                {
                    ErrorUpdate?.Invoke(this, true);
                }
            }
            else
            {
                ErrorUpdate?.Invoke(this, true);
            }
        }
        //private byte[] DataExchange_WriteScales()
        //{
        //    currentCommand = data_exchange;

        //    byte[] writeMessage = new byte[8];
        //    writeMessage[0] = startByte;
        //    writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
        //    writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
        //    writeMessage[3] = data_exchange;

        //    writeMessage[4] = (byte)currentAccelScale;
        //    writeMessage[5] = (byte)currentConductScale;

        //    var crc = CalculateCrc16Modbus(writeMessage);
        //    writeMessage[6] = crc[0];
        //    writeMessage[7] = crc[1];

        //    return writeMessage;
        //}

        ////установить шкалу Акселерометра
        //public void Set_AccelScale(AccelScale accelScale)
        //{
        //    currentAccelScale = accelScale;
        //}
        ////установить шкалу кондуктометр
        //public void Set_ConductScale(ConductScale conductScale)
        //{
        //    currentConductScale = conductScale;
        //}

  
        public byte[] Get_HW_Config()
        {
            currentCommand = get_HW_Config;

            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = get_HW_Config;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        }

        public void Answer_Get_HW_Config(byte[] buffer)
        {
            if (buffer[0] == startByte)
            {
                var length = new byte[2] { buffer[1], buffer[2] };
                if (buffer.Length == BitConverter.ToInt16(length, 0))
                {
                    if (buffer[3] == get_HW_Config && CheckAnswer(buffer))
                    {
                        //factoryCoeffs = new FactoryCoeffs();
                        //for (int i=0; i < 4; i++)
                        //{
                        //    factoryCoeffs.k[i] = BitConverter.ToSingle(buffer, 66 + i * 4);
                        //    factoryCoeffs.delta[i] = BitConverter.ToSingle(buffer, 82 + i * 4);
                        //}
                    }
                    else
                    {
                        ErrorUpdate?.Invoke(this, true);
                    }
                }
                else
                {
                    ErrorUpdate?.Invoke(this, true);
                }
            }
            else
            {
                ErrorUpdate?.Invoke(this, true);
            }
        }

        //Запись пользовательских калибровочных данных
        public byte[] Proto_msgType_set_UserData()
        {
            currentCommand = set_UserData;

            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = set_UserData;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        } 
        private void Answer_Proto_msgType_set_UserData(byte[] buffer)
        {
            if (buffer[0] == startByte)
            {
                var length = new byte[2] { buffer[1], buffer[2] };
                if (buffer.Length == BitConverter.ToInt16(length, 0))
                {
                    if (buffer[3] == set_UserData && CheckAnswer(buffer))
                    {
                        //тогда просто все ок
                    }
                    else
                    {
                        ErrorUpdate?.Invoke(this, true);
                    }
                }
                else
                {
                    ErrorUpdate?.Invoke(this, true);
                }
            }
            else
            {
                ErrorUpdate?.Invoke(this, true);
            }
        }

        //Чтение пользовательских калибровочных данных
        public byte[] Proto_msgType_get_UserData()
        {
            currentCommand = get_UserData;

            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = get_UserData;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        }
        private void Answer_Proto_msgType_get_UserData(byte[] buffer)
        {
            if (buffer[0] == startByte)
            {
                var length = new byte[2] { buffer[1], buffer[2] };
                if (buffer.Length == BitConverter.ToInt16(length, 0))
                {
                    if (buffer[3] == get_UserData && CheckAnswer(buffer))
                    {
                        
                    }
                    else
                    {
                        ErrorUpdate?.Invoke(this, true);
                    }
                }
                else
                {
                    ErrorUpdate?.Invoke(this, true);
                }
            }
            else
            {
                ErrorUpdate?.Invoke(this, true);
            }
        }
    }
}
