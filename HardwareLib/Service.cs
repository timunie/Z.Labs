using HardwareLib.Update;

using System.Diagnostics;
using Avalonia.Threading;
using HardwareLib.Classes;
using HardwareLib.ModbusCRC16;

namespace HardwareLib
{
    //сканирование всех доступных
    //подключение
    //тип лаборатории
    //обмен данными
    //построение графиков
    public class Service
    {
        private bool BLEConnection;

        public event EventHandler<Data> DataUpdate;
        public event EventHandler<EventsUpdateArgs> StatusUpdate;
        public event EventHandler<ArchiveInfoUpdateArgs> ArchiveInfoUpdate;
        DispatcherTimer timerForAnswer;
        private int retryNumber, busyFlag;

        bool dataExcange_Read = true;
        
        Command currentCommand;

        const byte unsupportMsg = 0xFF;
        const byte startByte = 0xAA;

        const byte data_exchange = 0x10;
        const byte get_HW_Config = 0x31;
        const byte get_UserData = 0x41;
        const byte set_UserData = 0x40;
        const byte proto_msgType_getInfo = 0x00;
        const byte archive_Control = 0x20;
        const byte getArchive = 0x21;
        private enum Command
        {
            data_exchange = 0x10,
            get_HW_Config = 0x31,
            get_UserData = 0x41,
            set_UserData = 0x40,
            proto_msgType_getInfo = 0x00,
            archive_Control = 0x20,
            getArchive = 0x21
        }
        private bool archiveInfo;
        private enum Step
        {
            doingCalibr,
            workWithArchive,
            readingData,
            writeScale,
            archiveStatus,
            setArchive,
            getArchive
        }
        private Step currentStep;

        AccelScale currentAccelScale;
        ConductScale currentConductScale;
        public enum ConductScale
        {
            k0 = 0,
            k1 = 1,
            k10 = 2,
            k100 = 3
        }
        public enum AccelScale
        {
            g2 = 0,
            g4 = 1,
            g8 = 2,
            g16 = 3
        }
        
        private const float _2g_coeff = 0.064F;
        private const float _4g_coeff = 0.128F;
        private const float _8g_coeff = 0.256F;
        private const float _16g_coeff = 0.512F;

        private int sensorInPageNumber; 

        public List<BLE_Dev> BLE_DevicesList, currentBLEList;
        public event EventHandler<List<BLE_Dev>> BleUpdate;

        public event EventHandler<int> TabSensorsUpdate;
        
        public event EventHandler<SensorListUpdateArgs> SensorListUpdate;
        public SensorList SensorList, currentList;

        // TODO: Implement this
        // BluetoothLEAdvertisementWatcher watcher;

        // GattDeviceService service = null;
        // GattCharacteristic charac = null;
        Guid MyService_GUID;
        Guid MYCharacteristic_GUID;
        Stopwatch stopwatch;

        const double defaultPeriod = 250;

        public event EventHandler<PlotUpdateArgs> TempSensorUpdate,
            AbsolutePressureSensorUpdate,
            TeslametrSensorUpdate,
            VoltmeterSensorUpdate,
            AmpermetrSensorUpdate,
            AccelerometerXSensorUpdate,
            AccelerometerYSensorUpdate,
            AccelerometerZSensorUpdate,
            ElectricalConductivitySensorUpdate,
            HumiditySensorUpdate,
            LightSensorUpdate,
            TempOutsideSensorUpdate,
            ColorimeterSensorUpdate;

        public event EventHandler<Plot_PH_UpdateArgs> PhSensorUpdate;
            

        private LabType labType;
        public LabType Get_LabType()
        {
            return labType;
        }

        DispatcherTimer timerMain, timerResearch;

        DispatcherTimer tempSensorTimer,
            absolutePressureSensorTimer,
            teslametrSensorTimer,
            voltmeterSensorTimer,
            ampermetrSensorTimer,
            accelerometerXSensorTimer,
            accelerometerYSensorTimer,
            accelerometerZSensorTimer,
            electricalConductivitySensorTimer,
            humiditySensorTimer,
            lightSensorTimer,
            tempOutsideSensorTimer,
            colorimeterSensorTimer,
            phSensorTimer;


        Data currentData, data_withFactoryCoef;
        List<byte> bytesToRead;

        PhSensor_Mode pHSensor_mode;

        FactoryCoeffs factoryCoeffs;

        UserCoeff_Bio userCoeff_Bio, currentCoeff_Bio;
        UserCalibrVal_BIO userCalibrValue_Bio;

        UserCoeff_Phys userCoeff_Phys, currentCoeff_Phys;
        UserCalibrVal_Phys userCalibrValue_Phys;

        UserCoeff_Chem userCoeff_Chem, currentCoeff_Chem;
        UserCalibrVal_Chem userCalibrValue_Chem;

        ArchiveSettings archiveSettingsFromDevice, archiveToStart;
        List<DataBoard> archiveDataFromDevice;
        int numberOfArchiveRecords, archiveNeedSteps, archiveCurrentStep;
        bool intArchiveRecord;

        public Service(int sensorNumbers)
        {
            sensorInPageNumber = sensorNumbers;

            timerMain = new DispatcherTimer();
            timerMain.Interval =  TimeSpan.FromMilliseconds(250);
            timerMain.Tick += TimerMain_Tick;

            timerResearch = new DispatcherTimer();
            timerResearch.Interval = TimeSpan.FromMilliseconds(3000);
            timerResearch.Tick += TimerResearch_Tick; 
            timerResearch.Start();

            #region Plot Timers
            tempSensorTimer = new DispatcherTimer();
            absolutePressureSensorTimer = new DispatcherTimer();
            teslametrSensorTimer = new DispatcherTimer();
            voltmeterSensorTimer = new DispatcherTimer();
            ampermetrSensorTimer = new DispatcherTimer();
            accelerometerXSensorTimer = new DispatcherTimer();
            accelerometerYSensorTimer = new DispatcherTimer();
            accelerometerZSensorTimer = new DispatcherTimer();
            electricalConductivitySensorTimer = new DispatcherTimer();
            humiditySensorTimer = new DispatcherTimer();
            lightSensorTimer = new DispatcherTimer();
            tempOutsideSensorTimer = new DispatcherTimer();
            colorimeterSensorTimer = new DispatcherTimer();
            phSensorTimer = new DispatcherTimer();


            //выставляем дефолтные настройки
            tempSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            absolutePressureSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            teslametrSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            voltmeterSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            ampermetrSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            accelerometerXSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            accelerometerYSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            accelerometerZSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            electricalConductivitySensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            humiditySensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            lightSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            tempOutsideSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            colorimeterSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            phSensorTimer.Interval = TimeSpan.FromMilliseconds(defaultPeriod);
            
            
            tempSensorTimer.Tick += TempSensorTimer_Tick;
            absolutePressureSensorTimer.Tick += AbsolutePressureSensorTimer_Tick;
            teslametrSensorTimer.Tick += TeslametrSensorTimer_Tick;
            voltmeterSensorTimer.Tick += VoltmeterSensorTimer_Tick;
            ampermetrSensorTimer.Tick += AmpermetrSensorTimer_Tick;
            accelerometerXSensorTimer.Tick += AccelerometerXSensorTimer_Tick;
            accelerometerYSensorTimer.Tick += AccelerometerYSensorTimer_Tick;
            accelerometerZSensorTimer.Tick += AccelerometerZSensorTimer_Tick;
            electricalConductivitySensorTimer.Tick += ElectricalConductivitySensorTimer_Tick;
            humiditySensorTimer.Tick += HumiditySensorTimer_Tick;
            lightSensorTimer.Tick += LightSensorTimer_Tick;
            tempOutsideSensorTimer.Tick += TempOutsideSensorTimer_Tick;
            colorimeterSensorTimer.Tick += ColorimeterSensorTimer_Tick;
            phSensorTimer.Tick += PhSensorTimer_Tick;
            #endregion

            stopwatch = new Stopwatch();

            BLE_DevicesList = new List<BLE_Dev>();

            MyService_GUID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
            MYCharacteristic_GUID = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");
            StartWatching();

            bytesToRead = new List<byte>();
            timerForAnswer = new DispatcherTimer();
            timerForAnswer.Tick += TimerForAnswer_Tick;
            timerForAnswer.Interval = TimeSpan.FromMilliseconds(50);

            currentList = new SensorList();
            busyFlag = 0;

            currentData = new Data();
            data_withFactoryCoef = new Data();

            factoryCoeffs = new FactoryCoeffs();
            
            userCoeff_Bio = new UserCoeff_Bio();
            currentCoeff_Bio = new UserCoeff_Bio();
            userCalibrValue_Bio = new UserCalibrVal_BIO();

            userCoeff_Phys = new UserCoeff_Phys();
            currentCoeff_Phys = new UserCoeff_Phys();
            userCalibrValue_Phys = new UserCalibrVal_Phys();

            userCoeff_Chem = new UserCoeff_Chem();
            currentCoeff_Chem = new UserCoeff_Chem();
            userCalibrValue_Chem = new UserCalibrVal_Chem();
        }

        public void Set_PhSensor_Mode(PhSensor_Mode mode)
        {
            pHSensor_mode = mode;
        }
        public PhSensor_Mode Get_PhSensor_Mode()
        {
            return pHSensor_mode;
        }

        //TODO: необходимо произвести запись в устройство
        #region Калибровка
        //датчик температры
        public void Set_UserCalibr_TempSensor_Value1(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x1_tempSensor = currentVal;
                    userCalibrValue_Bio.y1_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 1)
                    {
                        currentCoeff_Bio.step_TempSensor = 1;
                        currentCoeff_Bio.delta_TempSensor = userCalibrValue_Bio.y1_tempSensor - userCalibrValue_Bio.x1_tempSensor;
                    }
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCalibrValue_Chem.x1_tempSensor = currentVal;
                    userCalibrValue_Chem.y1_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 1)
                    {
                        currentCoeff_Chem.step_TempSensor = 1;
                        currentCoeff_Chem.delta_TempSensor = userCalibrValue_Chem.y1_tempSensor - userCalibrValue_Chem.x1_tempSensor;
                    }
                    break;
                #endregion

                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x1_tempSensor = currentVal;
                    userCalibrValue_Phys.y1_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 1)
                    {
                        currentCoeff_Phys.step_TempSensor = 1;
                        currentCoeff_Phys.delta_TempSensor = userCalibrValue_Phys.y1_tempSensor - userCalibrValue_Phys.x1_tempSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void Set_UserCalibr_TempSensor_Value2(float currentVal, ushort step)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x2_tempSensor = currentVal;
                    userCalibrValue_Bio.y2_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 2)
                    {
                        currentCoeff_Bio.step_TempSensor = 2;
                        currentCoeff_Bio.k_TempSensor = (userCalibrValue_Bio.y2_tempSensor - userCalibrValue_Bio.y1_tempSensor) / (userCalibrValue_Bio.x2_tempSensor - userCalibrValue_Bio.x1_tempSensor);
                        currentCoeff_Bio.delta_TempSensor = userCalibrValue_Bio.y1_tempSensor - userCoeff_Bio.k_TempSensor * userCalibrValue_Bio.x1_tempSensor;
                    }
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCalibrValue_Chem.x2_tempSensor = currentVal;
                    userCalibrValue_Chem.y2_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 2)
                    {
                        currentCoeff_Chem.step_TempSensor = 2;
                        currentCoeff_Chem.k_TempSensor = (userCalibrValue_Chem.y2_tempSensor - userCalibrValue_Chem.y1_tempSensor) / (userCalibrValue_Chem.x2_tempSensor - userCalibrValue_Chem.x1_tempSensor);
                        currentCoeff_Chem.delta_TempSensor = userCalibrValue_Chem.y1_tempSensor - userCoeff_Chem.k_TempSensor * userCalibrValue_Chem.x1_tempSensor;
                    }
                    break;
                #endregion

                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x2_tempSensor = currentVal;
                    userCalibrValue_Phys.y2_tempSensor = data_withFactoryCoef.TempSensor;

                    if (step == 2)
                    {
                        currentCoeff_Phys.step_TempSensor = 2;
                        currentCoeff_Phys.k_TempSensor = (userCalibrValue_Phys.y2_tempSensor - userCalibrValue_Phys.y1_tempSensor) / (userCalibrValue_Phys.x2_tempSensor - userCalibrValue_Phys.x1_tempSensor);
                        currentCoeff_Phys.delta_TempSensor = userCalibrValue_Phys.y1_tempSensor - userCoeff_Phys.k_TempSensor * userCalibrValue_Phys.x1_tempSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void DoingUserCalibr_TempSensor(ushort step)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.step_TempSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Bio.delta_TempSensor = userCalibrValue_Bio.y1_tempSensor - userCalibrValue_Bio.x1_tempSensor;
                            break;
                        case 2:
                            userCoeff_Bio.k_TempSensor = (userCalibrValue_Bio.y2_tempSensor - userCalibrValue_Bio.y1_tempSensor) / (userCalibrValue_Bio.x2_tempSensor - userCalibrValue_Bio.x1_tempSensor);
                            userCoeff_Bio.delta_TempSensor = userCalibrValue_Bio.y1_tempSensor - userCoeff_Bio.k_TempSensor * userCalibrValue_Bio.x1_tempSensor;
                            break;
                    }
                    break;
                #endregion

                #region Chem
                case LabType.chem:
                    userCoeff_Chem.step_TempSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Chem.delta_TempSensor = userCalibrValue_Chem.y1_tempSensor - userCalibrValue_Chem.x1_tempSensor;
                            break;
                        case 2:
                            userCoeff_Chem.k_TempSensor = (userCalibrValue_Chem.y2_tempSensor - userCalibrValue_Chem.y1_tempSensor) / (userCalibrValue_Chem.x2_tempSensor - userCalibrValue_Chem.x1_tempSensor);
                            userCoeff_Chem.delta_TempSensor = userCalibrValue_Chem.y1_tempSensor - userCoeff_Chem.k_TempSensor * userCalibrValue_Chem.x1_tempSensor;
                            break;
                    }
                    break;
                #endregion

                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_TempSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Phys.delta_TempSensor = userCalibrValue_Phys.y1_tempSensor - userCalibrValue_Phys.x1_tempSensor;
                            break;
                        case 2:
                            userCoeff_Phys.k_TempSensor = (userCalibrValue_Phys.y2_tempSensor - userCalibrValue_Phys.y1_tempSensor) / (userCalibrValue_Phys.x2_tempSensor - userCalibrValue_Phys.x1_tempSensor);
                            userCoeff_Phys.delta_TempSensor = userCalibrValue_Phys.y1_tempSensor - userCoeff_Phys.k_TempSensor * userCalibrValue_Phys.x1_tempSensor;
                            break;
                    }
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_TempSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.step_TempSensor = 1;
                    userCoeff_Bio.k_TempSensor = 1;
                    userCoeff_Bio.delta_TempSensor = 0;

                    currentCoeff_Bio.step_TempSensor = 1;
                    currentCoeff_Bio.k_TempSensor = 1;
                    currentCoeff_Bio.delta_TempSensor = 0;
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCoeff_Chem.step_TempSensor = 1;
                    userCoeff_Chem.k_TempSensor = 1;
                    userCoeff_Chem.delta_TempSensor = 0;

                    currentCoeff_Chem.step_TempSensor = 1;
                    currentCoeff_Chem.k_TempSensor = 1;
                    currentCoeff_Chem.delta_TempSensor = 0;
                    break;
                #endregion

                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_TempSensor = 1;
                    userCoeff_Phys.k_TempSensor = 1;
                    userCoeff_Phys.delta_TempSensor = 0;

                    currentCoeff_Phys.step_TempSensor = 1;
                    currentCoeff_Phys.k_TempSensor = 1;
                    currentCoeff_Phys.delta_TempSensor = 0;
                    break;
                    #endregion

            }

            //currentStep = Step.doingCalibr;
        }

        //датчик ph
        public void Set_UserCalibr_pHSensor_Value1(float currentVal, ushort step)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x1_pHSensor = currentVal;
                    userCalibrValue_Bio.y1_pHSensor = data_withFactoryCoef.PhSensor_inPh;

                    if (step == 1)
                    {
                        currentCoeff_Bio.step_pHSensor_inPH = 1;
                        currentCoeff_Bio.delta_pHSensor_inPH = userCalibrValue_Bio.y1_pHSensor - userCalibrValue_Bio.x1_pHSensor;
                    }
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCalibrValue_Chem.x1_pHSensor = currentVal;
                    userCalibrValue_Chem.y1_pHSensor = data_withFactoryCoef.PhSensor_inPh;

                    if (step == 1)
                    {
                        currentCoeff_Chem.step_pHSensor_inPH = 1;
                        currentCoeff_Chem.delta_pHSensor_inPH = userCalibrValue_Chem.y1_pHSensor - userCalibrValue_Chem.x1_pHSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void Set_UserCalibr_pH_Value2(float currentVal, ushort step)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x2_pHSensor = currentVal;
                    userCalibrValue_Bio.y2_pHSensor = data_withFactoryCoef.PhSensor_inPh;

                    if (step == 2)
                    {
                        currentCoeff_Bio.step_pHSensor_inPH = 2;
                        currentCoeff_Bio.k_pHSensor_inPH = (userCalibrValue_Bio.y2_pHSensor - userCalibrValue_Bio.y1_pHSensor) / (userCalibrValue_Bio.x2_pHSensor - userCalibrValue_Bio.x1_pHSensor);
                        currentCoeff_Bio.delta_pHSensor_inPH = userCalibrValue_Bio.y1_pHSensor - userCoeff_Bio.k_pHSensor_inPH * userCalibrValue_Bio.x1_pHSensor;
                    }
                    break;
                #endregion

                #region Chem
                case LabType.chem:
                    userCalibrValue_Chem.x2_pHSensor = currentVal;
                    userCalibrValue_Chem.y2_pHSensor = data_withFactoryCoef.PhSensor_inPh;

                    if (step == 2)
                    {
                        currentCoeff_Chem.step_pHSensor_inPH = 2;
                        currentCoeff_Chem.k_pHSensor_inPH = (userCalibrValue_Chem.y2_pHSensor - userCalibrValue_Chem.y1_pHSensor) / (userCalibrValue_Chem.x2_pHSensor - userCalibrValue_Chem.x1_pHSensor);
                        currentCoeff_Chem.delta_pHSensor_inPH = userCalibrValue_Chem.y1_pHSensor - userCoeff_Chem.k_pHSensor_inPH * userCalibrValue_Chem.x1_pHSensor;
                    }
                    break;
                    #endregion
            }

        }
        public void DoingUserCalibr_pHSensor(ushort step)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.step_pHSensor_inPH = step;

                    switch (step)
                    {
                        case 1:
                            userCoeff_Bio.delta_pHSensor_inPH = userCalibrValue_Bio.y1_pHSensor - userCalibrValue_Bio.x1_pHSensor;
                            break;
                        case 2:
                            userCoeff_Bio.k_pHSensor_inPH = (userCalibrValue_Bio.y2_pHSensor - userCalibrValue_Bio.y1_pHSensor) / (userCalibrValue_Bio.x2_pHSensor - userCalibrValue_Bio.x1_pHSensor);
                            userCoeff_Bio.delta_pHSensor_inPH = userCalibrValue_Bio.y1_pHSensor - userCoeff_Bio.k_pHSensor_inPH * userCalibrValue_Bio.x1_pHSensor;
                            break;
                    }
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCoeff_Chem.step_pHSensor_inPH = step;

                    switch (step)
                    {
                        case 1:
                            userCoeff_Chem.delta_pHSensor_inPH = userCalibrValue_Chem.y1_pHSensor - userCalibrValue_Chem.x1_pHSensor;
                            break;
                        case 2:
                            userCoeff_Chem.k_pHSensor_inPH = (userCalibrValue_Chem.y2_pHSensor - userCalibrValue_Chem.y1_pHSensor) / (userCalibrValue_Chem.x2_pHSensor - userCalibrValue_Chem.x1_pHSensor);
                            userCoeff_Chem.delta_pHSensor_inPH = userCalibrValue_Chem.y1_pHSensor - userCoeff_Chem.k_pHSensor_inPH * userCalibrValue_Chem.x1_pHSensor;
                            break;
                    }
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_pHSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.step_pHSensor_inPH = 1;
                    userCoeff_Bio.k_pHSensor_inPH = 1;
                    userCoeff_Bio.delta_pHSensor_inPH = 0;

                    currentCoeff_Bio.step_pHSensor_inPH = 1;
                    currentCoeff_Bio.k_pHSensor_inPH = 1;
                    currentCoeff_Bio.delta_pHSensor_inPH = 0;
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    userCoeff_Chem.step_pHSensor_inPH = 1;
                    userCoeff_Chem.k_pHSensor_inPH = 1;
                    userCoeff_Chem.delta_pHSensor_inPH = 0;

                    currentCoeff_Chem.step_pHSensor_inPH = 1;
                    currentCoeff_Chem.k_pHSensor_inPH = 1;
                    currentCoeff_Chem.delta_pHSensor_inPH = 0;
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }

        //датчик влажности
        public void Set_UserCalibr_HumiditySensor_Value1(float currentVal)
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x1_HumiditySensor = currentVal;
                    userCalibrValue_Bio.y1_HumiditySensor = data_withFactoryCoef.HumiditySensor;

                    currentCoeff_Bio.delta_HumiditySensor = userCalibrValue_Bio.y1_HumiditySensor - userCalibrValue_Bio.x1_HumiditySensor;
                    break;
                    #endregion
            }
        }
        public void DoingUserCalibr_HumiditySensor()
        {
            switch(labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_HumiditySensor = userCalibrValue_Bio.y1_HumiditySensor - userCalibrValue_Bio.x1_HumiditySensor;
                    break;
                #endregion
            }
            
            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_HumiditySensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_HumiditySensor = 0;
                    currentCoeff_Bio.delta_HumiditySensor = 0;
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }

        //датчик освещенности
        public void Set_UserCalibr_LightSensor_Value1(float currentVal)
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x1_LightSensor = currentVal;
                    userCalibrValue_Bio.y1_LightSensor = data_withFactoryCoef.LightSensor;

                    currentCoeff_Bio.delta_LightSensor = userCalibrValue_Bio.y1_LightSensor - userCalibrValue_Bio.x1_LightSensor;
                    break;
                    #endregion
            }
            
        }
        public void DoingUserCalibr_LightSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_LightSensor = userCalibrValue_Bio.y1_LightSensor - userCalibrValue_Bio.x1_LightSensor;
                    break;
                    #endregion
            }
            
            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_LightSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_LightSensor = 0;
                    currentCoeff_Bio.delta_LightSensor = 0;
                    break;
                    #endregion
            }
            
            //currentStep = Step.doingCalibr;
        }

        //датчик Tout
        public void Set_UserCalibr_TempOutsideSensor_Value1(float currentVal)
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCalibrValue_Bio.x1_ToutSensor = currentVal;
                    userCalibrValue_Bio.y1_ToutSensor = data_withFactoryCoef.TempOutsideSensor;

                    currentCoeff_Bio.delta_ToutSensor = userCalibrValue_Bio.y1_ToutSensor - userCalibrValue_Bio.x1_ToutSensor;
                    break;
                    #endregion
            }
            
        }
        public void DoingUserCalibr_TempOutsideSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_ToutSensor = userCalibrValue_Bio.y1_ToutSensor - userCalibrValue_Bio.x1_ToutSensor;
                    break;
                    #endregion
            }
            
            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_TempOutsideSensor()
        {
            switch (labType)
            {
                #region BIO
                case LabType.bio:
                    userCoeff_Bio.delta_ToutSensor = 0;
                    currentCoeff_Bio.delta_ToutSensor = 0;
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }

        //датчик электропроводимости
        public void Set_UserCalibr_ElectricalConductivitySensor_Value1(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region CHEM
                case LabType.chem:
                    userCalibrValue_Chem.x1_ElectricalConductivitySensor = currentVal;
                    userCalibrValue_Chem.y1_ElectricalConductivitySensor = data_withFactoryCoef.ElectricalConductivitySensor;

                    if (step == 1)
                    {
                        currentCoeff_Chem.step_ElectricalConductivitySensor = 1;
                        currentCoeff_Chem.delta_ElectricalConductivitySensor = userCalibrValue_Chem.y1_ElectricalConductivitySensor - userCalibrValue_Chem.x1_ElectricalConductivitySensor;
                    }
                    break;
                #endregion
            }
        }
        public void Set_UserCalibr_ElectricalConductivitySensor_Value2(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region CHEM
                case LabType.chem:
                    userCalibrValue_Chem.x2_ElectricalConductivitySensor = currentVal;
                    userCalibrValue_Chem.y2_ElectricalConductivitySensor = data_withFactoryCoef.ElectricalConductivitySensor;

                    if (step == 2)
                    {
                        currentCoeff_Chem.step_ElectricalConductivitySensor = 2;
                        currentCoeff_Chem.k_ElectricalConductivitySensor = (userCalibrValue_Chem.y2_ElectricalConductivitySensor - userCalibrValue_Chem.y1_ElectricalConductivitySensor) / (userCalibrValue_Chem.x2_ElectricalConductivitySensor - userCalibrValue_Chem.x1_ElectricalConductivitySensor);
                        currentCoeff_Chem.delta_ElectricalConductivitySensor = userCalibrValue_Chem.y1_ElectricalConductivitySensor - userCoeff_Chem.k_ElectricalConductivitySensor * userCalibrValue_Chem.x1_ElectricalConductivitySensor;
                    }
                    break;
                #endregion

            }
        }
        public void DoingUserCalibr_ElectricalConductivitySensor(ushort step)
        {
            switch (labType)
            {
                #region Chem
                case LabType.chem:
                    userCoeff_Chem.step_ElectricalConductivitySensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Chem.delta_ElectricalConductivitySensor = userCalibrValue_Chem.y1_ElectricalConductivitySensor - userCalibrValue_Chem.x1_ElectricalConductivitySensor;
                            break;
                        case 2:
                            userCoeff_Chem.k_ElectricalConductivitySensor = (userCalibrValue_Chem.y2_ElectricalConductivitySensor - userCalibrValue_Chem.y1_ElectricalConductivitySensor) / (userCalibrValue_Chem.x2_ElectricalConductivitySensor - userCalibrValue_Chem.x1_ElectricalConductivitySensor);
                            userCoeff_Chem.delta_ElectricalConductivitySensor = userCalibrValue_Chem.y1_ElectricalConductivitySensor - userCoeff_Chem.k_ElectricalConductivitySensor * userCalibrValue_Chem.x1_ElectricalConductivitySensor;
                            break;
                    }
                    break;
                #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_ElectricalConductivitySensor()
        {
            switch (labType)
            {
                #region CHEM
                case LabType.chem:
                    userCoeff_Chem.step_ElectricalConductivitySensor = 1;
                    userCoeff_Chem.k_ElectricalConductivitySensor = 1;
                    userCoeff_Chem.delta_ElectricalConductivitySensor = 0;

                    currentCoeff_Chem.step_ElectricalConductivitySensor = 1;
                    currentCoeff_Chem.k_ElectricalConductivitySensor = 1;
                    currentCoeff_Chem.delta_ElectricalConductivitySensor = 0;
                    break;
                #endregion
            }

            //currentStep = Step.doingCalibr;
        }

        //датчик Датчик P абс
        public void Set_UserCalibr_AbsolutePressureSensor_Value1(float currentVal)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x1_AbsolutePressure = currentVal;
                    userCalibrValue_Phys.y1_AbsolutePressure = data_withFactoryCoef.AbsolutePressureSensor;

                    currentCoeff_Phys.delta_AbsolutePressure = userCalibrValue_Phys.y1_AbsolutePressure - userCalibrValue_Phys.x1_AbsolutePressure;
                    break;
                    #endregion
            }

        }
        public void DoingUserCalibr_AbsolutePressureSensor()
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.delta_AbsolutePressure = userCalibrValue_Phys.y1_AbsolutePressure - userCalibrValue_Phys.y1_AbsolutePressure;
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_AbsolutePressureSensor()
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.delta_AbsolutePressure = 0;
                    currentCoeff_Phys.delta_AbsolutePressure = 0;
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }

        //тесламетр
        public void Set_UserCalibr_TeslametrSensor_Value1(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x1_TeslametrSensor = currentVal;
                    userCalibrValue_Phys.y1_TeslametrSensor = data_withFactoryCoef.TeslametrSensor;

                    if (step == 1)
                    {
                        currentCoeff_Phys.step_TeslametrSensor = 1;
                        currentCoeff_Phys.delta_TeslametrSensor = userCalibrValue_Phys.y1_TeslametrSensor - userCalibrValue_Phys.x1_TeslametrSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void Set_UserCalibr_TeslametrSensor_Value2(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x2_TeslametrSensor = currentVal;
                    userCalibrValue_Phys.y2_TeslametrSensor = data_withFactoryCoef.TeslametrSensor;

                    if (step == 2)
                    {
                        currentCoeff_Phys.step_TeslametrSensor = 2;
                        currentCoeff_Phys.k_TeslametrSensor = (userCalibrValue_Phys.y2_TeslametrSensor - userCalibrValue_Phys.y1_TeslametrSensor) / (userCalibrValue_Phys.x2_TeslametrSensor - userCalibrValue_Phys.x1_TeslametrSensor);
                        currentCoeff_Phys.delta_TeslametrSensor = userCalibrValue_Phys.y1_TeslametrSensor - userCoeff_Phys.k_TeslametrSensor * userCalibrValue_Phys.x1_TeslametrSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void DoingUserCalibr_TeslametrSensor(ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_TeslametrSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Phys.delta_AmpermetrSensor = userCalibrValue_Phys.y1_AmpermetrSensor - userCalibrValue_Phys.x1_TeslametrSensor;
                            break;
                        case 2:
                            userCoeff_Phys.k_TeslametrSensor = (userCalibrValue_Phys.y2_TeslametrSensor - userCalibrValue_Phys.y1_TeslametrSensor) / (userCalibrValue_Phys.x2_TeslametrSensor - userCalibrValue_Phys.x2_TeslametrSensor);
                            userCoeff_Phys.delta_TeslametrSensor = userCalibrValue_Phys.y1_TeslametrSensor - userCoeff_Phys.k_TeslametrSensor * userCalibrValue_Phys.x1_TeslametrSensor;
                            break;
                    }
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_TeslametrSensor()
        {
            switch (labType)
            {

                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_TeslametrSensor = 1;
                    userCoeff_Phys.k_TeslametrSensor = 1;
                    userCoeff_Phys.delta_TeslametrSensor = 0;

                    currentCoeff_Phys.step_TeslametrSensor = 1;
                    currentCoeff_Phys.k_TeslametrSensor = 1;
                    currentCoeff_Phys.delta_TeslametrSensor = 0;
                    break;
                    #endregion

            }

            //currentStep = Step.doingCalibr;
        }

        //вольтметр
        public void Set_UserCalibr_VoltmeterSensor_Value1(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x1_VoltmeterSensor = currentVal;
                    userCalibrValue_Phys.y1_VoltmeterSensor = data_withFactoryCoef.VoltmeterSensor;

                    if (step == 1)
                    {
                        currentCoeff_Phys.step_VoltmeterSensor = 1;
                        currentCoeff_Phys.delta_VoltmeterSensor = userCalibrValue_Phys.y1_VoltmeterSensor - userCalibrValue_Phys.x1_VoltmeterSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void Set_UserCalibr_VoltmeterSensor_Value2(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x2_VoltmeterSensor = currentVal;
                    userCalibrValue_Phys.y2_VoltmeterSensor = data_withFactoryCoef.VoltmeterSensor;

                    if (step == 2)
                    {
                        currentCoeff_Phys.step_VoltmeterSensor = 2;
                        currentCoeff_Phys.k_VoltmeterSensor = (userCalibrValue_Phys.y2_VoltmeterSensor - userCalibrValue_Phys.y1_VoltmeterSensor) / (userCalibrValue_Phys.x2_VoltmeterSensor - userCalibrValue_Phys.x1_VoltmeterSensor);
                        currentCoeff_Phys.delta_VoltmeterSensor = userCalibrValue_Phys.y1_VoltmeterSensor - userCoeff_Phys.k_VoltmeterSensor * userCalibrValue_Phys.x1_VoltmeterSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void DoingUserCalibr_VoltmeterSensor(ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_VoltmeterSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Phys.delta_VoltmeterSensor = userCalibrValue_Phys.y1_VoltmeterSensor - userCalibrValue_Phys.x1_VoltmeterSensor;
                            break;
                        case 2:
                            userCoeff_Phys.k_VoltmeterSensor = (userCalibrValue_Phys.y2_VoltmeterSensor - userCalibrValue_Phys.y1_VoltmeterSensor) / (userCalibrValue_Phys.x2_VoltmeterSensor - userCalibrValue_Phys.x1_VoltmeterSensor);
                            userCoeff_Phys.delta_VoltmeterSensor = userCalibrValue_Phys.y1_VoltmeterSensor - userCoeff_Phys.k_VoltmeterSensor * userCalibrValue_Phys.x1_VoltmeterSensor;
                            break;
                    }
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_VoltmeterSensor()
        {
            switch (labType)
            {

                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_VoltmeterSensor = 1;
                    userCoeff_Phys.k_VoltmeterSensor = 1;
                    userCoeff_Phys.delta_VoltmeterSensor = 0;

                    currentCoeff_Phys.step_VoltmeterSensor = 1;
                    currentCoeff_Phys.k_VoltmeterSensor = 1;
                    currentCoeff_Phys.delta_VoltmeterSensor = 0;
                    break;
                    #endregion

            }

            //currentStep = Step.doingCalibr;
        }

        //амперметр
        public void Set_UserCalibr_AmpermetrSensor_Value1(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x1_AmpermetrSensor = currentVal;
                    userCalibrValue_Phys.y1_AmpermetrSensor = data_withFactoryCoef.AmpermetrSensor;

                    if (step == 1)
                    {
                        currentCoeff_Phys.step_AmpermetrSensor = 1;
                        currentCoeff_Phys.delta_AmpermetrSensor = userCalibrValue_Phys.y1_AmpermetrSensor - userCalibrValue_Phys.x1_AmpermetrSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void Set_UserCalibr_AmpermetrSensor_Value2(float currentVal, ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCalibrValue_Phys.x2_AmpermetrSensor = currentVal;
                    userCalibrValue_Phys.y2_AmpermetrSensor = data_withFactoryCoef.AmpermetrSensor;

                    if (step == 2)
                    {
                        currentCoeff_Phys.step_AmpermetrSensor = 2;
                        currentCoeff_Phys.k_AmpermetrSensor = (userCalibrValue_Phys.y2_AmpermetrSensor - userCalibrValue_Phys.y1_AmpermetrSensor) / (userCalibrValue_Phys.x2_AmpermetrSensor - userCalibrValue_Phys.x1_AmpermetrSensor);
                        currentCoeff_Phys.delta_AmpermetrSensor = userCalibrValue_Phys.y1_AmpermetrSensor - userCoeff_Phys.k_AmpermetrSensor * userCalibrValue_Phys.x1_AmpermetrSensor;
                    }
                    break;
                    #endregion
            }
        }
        public void DoingUserCalibr_AmpermetrSensor(ushort step)
        {
            switch (labType)
            {
                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_AmpermetrSensor = step;
                    switch (step)
                    {
                        case 1:
                            userCoeff_Phys.delta_AmpermetrSensor = userCalibrValue_Phys.y1_AmpermetrSensor - userCalibrValue_Phys.x1_AmpermetrSensor;
                            break;
                        case 2:
                            userCoeff_Phys.k_AmpermetrSensor = (userCalibrValue_Phys.y2_AmpermetrSensor - userCalibrValue_Phys.y1_AmpermetrSensor) / (userCalibrValue_Phys.x2_AmpermetrSensor - userCalibrValue_Phys.x1_AmpermetrSensor);
                            userCoeff_Phys.delta_AmpermetrSensor = userCalibrValue_Phys.y1_AmpermetrSensor - userCoeff_Phys.k_AmpermetrSensor * userCalibrValue_Phys.x1_AmpermetrSensor;
                            break;
                    }
                    break;
                    #endregion
            }

            //currentStep = Step.doingCalibr;
        }
        public void Reset_UserCalibr_AmpermetrSensor()
        {
            switch (labType)
            {

                #region PHYS
                case LabType.phys:
                    userCoeff_Phys.step_AmpermetrSensor = 1;
                    userCoeff_Phys.k_AmpermetrSensor = 1;
                    userCoeff_Phys.delta_AmpermetrSensor = 0;

                    currentCoeff_Phys.step_AmpermetrSensor = 1;
                    currentCoeff_Phys.k_AmpermetrSensor = 1;
                    currentCoeff_Phys.delta_AmpermetrSensor = 0;
                    break;
                    #endregion

            }

            //currentStep = Step.doingCalibr;
        }
        #endregion

        #region Vis List
        private void SensorDependsOnLabVisibility()
        {
            SensorList = new SensorList();
            SensorList.Visibility = new bool[sensorInPageNumber];
            for (int i = 0; i < sensorInPageNumber; i++)
            {
                SensorList.Visibility[i] = false;
            }

            switch (labType)
            {
                case LabType.phys:
                    SensorList.Visibility[(int)SensorType.TempSensor] = true;
                    SensorList.Visibility[(int)SensorType.AbsolutePressureSensor] = true;
                    SensorList.Visibility[(int)SensorType.TeslametrSensor] = true;
                    SensorList.Visibility[(int)SensorType.VoltmeterSensor] = true;
                    SensorList.Visibility[(int)SensorType.AmpermetrSensor] = true;
                    SensorList.Visibility[(int)SensorType.AccelerometerXSensor] = true;
                    SensorList.Visibility[(int)SensorType.AccelerometerYSensor] = true;
                    SensorList.Visibility[(int)SensorType.AccelerometerZSensor] = true;
                    break;
                case LabType.chem:
                    SensorList.Visibility[(int)SensorType.TempSensor] = true;
                    SensorList.Visibility[(int)SensorType.ElectricalConductivitySensor] = true;
                    SensorList.Visibility[(int)SensorType.PhSensor] = true;
                    break;
                case LabType.eco:
                    
                    break;
                case LabType.bio:
                    SensorList.Visibility[(int)SensorType.TempSensor] = true;
                    SensorList.Visibility[(int)SensorType.HumiditySensor] = true;
                    SensorList.Visibility[(int)SensorType.LightSensor] = true;
                    SensorList.Visibility[(int)SensorType.TempOutsideSensor] = true;
                    SensorList.Visibility[(int)SensorType.PhSensor] = true;
                    break;
            }
            currentList = SensorList;
            SensorListUpdate?.Invoke(this, new SensorListUpdateArgs(SensorList));
        }

        public bool[] CurentSensorList_Vis()
        {
            bool[] vis = new bool[currentList.Visibility.Length];
            for(int i=0; i < currentList.Visibility.Length; i++)
            {
                var val = false;
                if (currentList.Visibility[i] == true)
                    val = true;

                vis[i] = val;
            }
            return vis;
        }
        public void SensorVisibility(SensorType sensor, bool isVisible)
        {

            SensorList = new SensorList();
            SensorList.Visibility = new bool[sensorInPageNumber];
            for (int i = 0; i < sensorInPageNumber; i++)
            {
                SensorList.Visibility[i] = currentList.Visibility[i];
            }

            SensorList.Visibility[(int)sensor] = isVisible;
            currentList = SensorList;
            SensorListUpdate?.Invoke(this, new SensorListUpdateArgs(SensorList));

        }
        public void SetDefaultVisibility()
        {
            SensorList = new SensorList();
            SensorList.Visibility = new bool[sensorInPageNumber];
            for (int i = 0; i < sensorInPageNumber; i++)
            {
                SensorList.Visibility[i] = false;
            }
            currentList = SensorList;
            SensorListUpdate?.Invoke(this, new SensorListUpdateArgs(SensorList));
        }
        #endregion

        #region Plots Timer Interval + Update
        public void StartPlotsTimer()
        {
            tempSensorTimer.Start();
            phSensorTimer.Start();
            electricalConductivitySensorTimer.Start();
            humiditySensorTimer.Start();
            lightSensorTimer.Start();
            tempOutsideSensorTimer.Start();
            absolutePressureSensorTimer.Start();
            teslametrSensorTimer.Start();
            voltmeterSensorTimer.Start();
            ampermetrSensorTimer.Start();
            accelerometerXSensorTimer.Start();
            accelerometerYSensorTimer.Start();
            accelerometerZSensorTimer.Start();
        }
        public void StopPlotsTimer()
        {
            tempSensorTimer.Stop();
            phSensorTimer.Stop();
            electricalConductivitySensorTimer.Stop();
            humiditySensorTimer.Stop();
            lightSensorTimer.Stop();
            tempOutsideSensorTimer.Stop();
            absolutePressureSensorTimer.Stop();
            teslametrSensorTimer.Stop();
            voltmeterSensorTimer.Stop();
            ampermetrSensorTimer.Stop();
            accelerometerXSensorTimer.Stop();
            accelerometerYSensorTimer.Stop();
            accelerometerZSensorTimer.Stop();
        }
        public void Change_Period_tempSensorTimer(double Milliseconds)
        {
            tempSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_absolutePressureSensorTimer(double Milliseconds)
        {
            absolutePressureSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_teslametrSensorTimer(double Milliseconds)
        {
            teslametrSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_voltmeterSensorTimer(double Milliseconds)
        {
            voltmeterSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_ampermetrSensorTimer(double Milliseconds)
        {
            ampermetrSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_accelerometerXSensorTimer(double Milliseconds)
        {
            accelerometerXSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_accelerometerYSensorTimer(double Milliseconds)
        {
            accelerometerYSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_accelerometerZSensorTimer(double Milliseconds)
        {
            accelerometerZSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }

        public void Change_Period_electricalConductivitySensorTimer(double Milliseconds)
        {
            electricalConductivitySensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_humiditySensorTimer(double Milliseconds)
        {
            humiditySensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_lightSensorTimer(double Milliseconds)
        {
            lightSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_tempOutsideSensorTimer(double Milliseconds)
        {
            tempOutsideSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_colorimeterSensorTimer(double Milliseconds)
        {
            colorimeterSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }
        public void Change_Period_phSensorTimer(double Milliseconds)
        {
            phSensorTimer.Interval = TimeSpan.FromMilliseconds(Milliseconds);
        }

        private void TempSensorTimer_Tick(object sender, EventArgs e)
        {
            TempSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.TempSensor));
        }
        private void AbsolutePressureSensorTimer_Tick(object sender, EventArgs e)
        {
            AbsolutePressureSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.AbsolutePressureSensor));
        }
        private void TeslametrSensorTimer_Tick(object sender, EventArgs e)
        {
            TeslametrSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.TeslametrSensor));
        }
        private void VoltmeterSensorTimer_Tick(object sender, EventArgs e)
        {
            VoltmeterSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.VoltmeterSensor));
        }
        private void AmpermetrSensorTimer_Tick(object sender, EventArgs e)
        {
            AmpermetrSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.AmpermetrSensor));
        }
        private void AccelerometerZSensorTimer_Tick(object sender, EventArgs e)
        {
            AccelerometerZSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.AccelerometerZSensor));
        }
        private void AccelerometerYSensorTimer_Tick(object sender, EventArgs e)
        {
            AccelerometerYSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.AccelerometerYSensor));
        }
        private void AccelerometerXSensorTimer_Tick(object sender, EventArgs e)
        {
            AccelerometerXSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.AccelerometerXSensor));
        }
        private void ElectricalConductivitySensorTimer_Tick(object sender, EventArgs e)
        {
            ElectricalConductivitySensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.ElectricalConductivitySensor));
        }
        private void HumiditySensorTimer_Tick(object sender, EventArgs e)
        {
            HumiditySensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.HumiditySensor));
        }
        private void LightSensorTimer_Tick(object sender, EventArgs e)
        {
            LightSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.LightSensor));
        }
        private void TempOutsideSensorTimer_Tick(object sender, EventArgs e)
        {
            TempOutsideSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.TempOutsideSensor));
        }
        private void ColorimeterSensorTimer_Tick(object sender, EventArgs e)
        {
            ColorimeterSensorUpdate?.Invoke(this, new PlotUpdateArgs(currentData.ColorimeterSensor));
        }
        private void PhSensorTimer_Tick(object sender, EventArgs e)
        {
            PhSensorUpdate?.Invoke(this, new Plot_PH_UpdateArgs(currentData.PhSensor_inVolt, currentData.PhSensor_inPh));
        }
        #endregion

        #region BLE research
        private void StartWatching()
        {
            // TODO: Implement this

            // watcher = new BluetoothLEAdvertisementWatcher
            // {
            //     ScanningMode = BluetoothLEScanningMode.Active
            // };
            // watcher.Received += OnAdvertisementReceivedAsync;
            // stopwatch.Start();
            // watcher.Start();
        }
        private void TimerResearch_Tick(object sender, EventArgs e)
        {
            // TODO: Implement this
            // watcher.Stop();
            BleUpdate?.Invoke(this, BLE_DevicesList);
        }
        public void RefreshBleDev()
        {
            BLE_DevicesList.Clear();
            // TODO: Implement this
            // watcher.Start();
        }
        public void StopWatcher()
        {
            // TODO: Implement this
            // watcher.Stop();
            stopwatch.Stop();
        }
        // TODO: Implement this
        // private async void OnAdvertisementReceivedAsync(BluetoothLEAdvertisementWatcher watcher,
        //                                              BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        // {
        //     //device name
        //     string devName = eventArgs.Advertisement.LocalName;
        //     var device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
        //
        //     if (device != null)
        //     {
        //         var rssi = eventArgs.RawSignalStrengthInDBm;
        //         var bleAddress = eventArgs.BluetoothAddress;
        //         var advertisementType = eventArgs.AdvertisementType;
        //
        //         //TODO: делаем условие по названиям CHEM и тд
        //         if (devName != "")
        //         {
        //             if (BLE_DevicesList.FindAll(dev => dev.DevName.Equals(devName)).Count > 0)
        //             {
        //                 BLE_DevicesList.FindAll(dev => dev.DevName.Equals(devName)).ForEach(x => x.rssi = rssi);
        //             }
        //             else
        //                 BLE_DevicesList.Add(new BLE_Dev() { BleDevice = device, rssi = rssi, DevName = devName });
        //         }
        //     }
        //
        // }

        #endregion


        private void TimerMain_Tick(object sender, EventArgs e)
        {
            if (busyFlag == 0)
            {
                try
                {
                    switch(currentStep)
                    {
                        case Step.readingData:
                            dataExcange_Read = true;
                            Send(DataExchange_Read());
                            break;

                        case Step.doingCalibr:
                            Send(Proto_msgType_set_UserData());
                            break;

                        case Step.writeScale:
                            dataExcange_Read = false;
                            Send(DataExchange_WriteScales());
                            break;

                        case Step.archiveStatus:
                            Send(ArchiveControl_Info());
                            break;

                        case Step.setArchive:
                            Send(ArchiveControl_Start(archiveToStart));
                            break;

                        case Step.getArchive:
                            //целое число
                            if(intArchiveRecord)
                            {
                                Send(Archive_Get((byte)(archiveCurrentStep * 4), 4));
                                archiveCurrentStep ++;                                   
                            }                          
                            break;


                    }
                    
                }
                catch { }
            }
                
        }       
        public bool GetStatus_BLEConnecttion()
        {
            return BLEConnection;
        }
        
        // TODO: Implement this
        // public async void ConnectDevice(Device device)
        // {
        //     try
        //     {
        //         var result = await device.GetGattServicesForUuidAsync(MyService_GUID);
        //         if (result.Status == GattCommunicationStatus.Success)
        //         {
        //             var services = result.Services;
        //             service = services[0];
        //             if (service != null)
        //             {
        //                 var charResult = await service.GetCharacteristicsForUuidAsync(MYCharacteristic_GUID);
        //                 if (charResult.Status == GattCommunicationStatus.Success)
        //                 {
        //                     charac = charResult.Characteristics[0];
        //                     if (charac != null)
        //                     {
        //                         var descriptorValue = GattClientCharacteristicConfigurationDescriptorValue.None;
        //                         GattCharacteristicProperties properties = charac.CharacteristicProperties;
        //                         string descriptor = string.Empty;
        //
        //                         if (properties.HasFlag(GattCharacteristicProperties.Notify))
        //                         {
        //                             descriptor = "notifications";
        //                             descriptorValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
        //                         }
        //                         try
        //                         {
        //                             var descriptorWriteResult = await charac.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(descriptorValue);
        //
        //                             if (descriptorWriteResult.Status == GattCommunicationStatus.Success)
        //                             {
        //                                 BLEConnection = true;
        //
        //                                 charac.ValueChanged += Charac_ValueChangedAsync;
        //
        //                                 Send(Proto_msgType_getInfo());
        //                               
        //                             }
        //                             else
        //                             {
        //                                 StatusUpdate?.Invoke(this, new EventsUpdateArgs("Ошибка при подключении к устройству", true));
        //                             }
        //                         }
        //                         catch (UnauthorizedAccessException ex)
        //                         {
        //                             StatusUpdate?.Invoke(this, new EventsUpdateArgs("Ошибка при подключении к устройству", true));
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             StatusUpdate?.Invoke(this, new EventsUpdateArgs("Ошибка при подключении к устройству", true));
        //         }
        //     }
        //
        //     catch
        //     {
        //         StatusUpdate?.Invoke(this, new EventsUpdateArgs("Ошибка при подключении к устройству", true));
        //     }
        // }
        public async void Send(byte[] arrayWrite)
        {
            // TODO: Implement this
            // IBuffer buffer = arrayWrite.AsBuffer();
            // var writeBuffer = CryptographicBuffer.CreateFromByteArray(arrayWrite);
            //
            // busyFlag = 10;
            // timerForAnswer.Start();
            // bytesToRead = new List<byte>();
            //
            // try
            // {
            //     var res = await charac.WriteValueAsync(writeBuffer);
            //
            //     if (res != GattCommunicationStatus.Success)
            //     {
            //         StatusUpdate?.Invoke(this, new EventsUpdateArgs("Устройство не доступно", true));
            //     }
            // }
            // catch (Exception ex) 
            // {
            //     StatusUpdate?.Invoke(this, new EventsUpdateArgs(ex.ToString(), true));
            // }
        }

        private void TimerForAnswer_Tick(object sender, EventArgs e)
        {
            if (busyFlag >= 10 && busyFlag < 20) busyFlag++; // retry
            if (busyFlag == 13)
            {
                // сообщение об ошибке получения данных
                BLEConnection = false;
                timerForAnswer.Stop();
                StatusUpdate?.Invoke(this, new EventsUpdateArgs("Устройство не отвечает", true));
                timerMain.Stop();
                labType = 0;
                SetDefaultVisibility();
                TabSensorsUpdate?.Invoke(this, (int)labType);
            }
            if (busyFlag == 20) // есть данные за эту итерацию
            {
                if (Parser()) { busyFlag = 0; timerForAnswer.Stop(); } // успех
                else busyFlag = 10;
            }
        }
        
        // TODO: Implement this
        // private async void Charac_ValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
        // {
        //     timerForAnswer.Start();
        //     busyFlag = 20;
        //     CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out byte[] data);
        //     try
        //     {
        //         var d = data;
        //         for(int i=0; i<d.Length; i++)
        //         {
        //             bytesToRead.Add(d[i]);  
        //         }
        //     }
        //     catch (ArgumentException)
        //     {
        //         MessageBox.Show("Unknown format");
        //     }
        // }
        private bool Parser()
        {
            bool success = false;
            for(int i=0; i< bytesToRead.Count(); i++)
            {
                if(bytesToRead[i] == startByte && bytesToRead.Count > 4)
                {
                    var lengthByte = new byte[2] { bytesToRead[i + 1], bytesToRead[i + 2] };
                    var length = BitConverter.ToInt16(lengthByte, 0);

                    if (bytesToRead.Count() - i >= length)
                    {
                        //вычленить массив и проверить crc => после уже смотрим саму посылку
                        byte[] massive = new byte[length];
                        for(int l=0; l< length; l++)
                        {
                            massive[l] = bytesToRead[i + l];
                        }
                        if(CheckAnswer(massive) & (byte)currentCommand == massive[3])
                        {
                            success = true;

                            switch (currentCommand)
                            {
                                #region узнать тип лаборатории
                                case Command.proto_msgType_getInfo:
                                    labType = (LabType)massive[10];
                                    SensorDependsOnLabVisibility();
                                    TabSensorsUpdate?.Invoke(this, (int)labType);

                                    Send(Get_HW_Config());
                                    break;
                                #endregion

                                #region узнать заводские коэффициенты
                                case Command.get_HW_Config:
                                    factoryCoeffs.k_Channel0 = BitConverter.ToSingle(massive, 30);
                                    factoryCoeffs.delta_Channel0 = BitConverter.ToSingle(massive, 34);
                                    factoryCoeffs.k_Channel1 = BitConverter.ToSingle(massive, 38);
                                    factoryCoeffs.delta_Channel1 = BitConverter.ToSingle(massive, 42);
                                    factoryCoeffs.k_Channel2 = BitConverter.ToSingle(massive, 46);
                                    factoryCoeffs.delta_Channel2 = BitConverter.ToSingle(massive, 50);
                                    factoryCoeffs.k_Channel3 = BitConverter.ToSingle(massive, 54);
                                    factoryCoeffs.delta_Channel3 = BitConverter.ToSingle(massive, 58);
                                    factoryCoeffs.delta_HumiditySensor = BitConverter.ToSingle(massive, 62);
                                    factoryCoeffs.delta_LightSensor = BitConverter.ToSingle(massive, 66);
                                    factoryCoeffs.delta_ToutSensor = BitConverter.ToSingle(massive, 70);
                                    factoryCoeffs.delta_AbsolutePressureSensor = BitConverter.ToSingle(massive, 74);
                                    factoryCoeffs.k_AmpermetrSensor = BitConverter.ToSingle(massive, 78);
                                    factoryCoeffs.delta_AmpermetrSensor = BitConverter.ToSingle(massive, 82);
                                    factoryCoeffs.k_VoltmeterSensor = BitConverter.ToSingle(massive, 86);
                                    factoryCoeffs.delta_VoltmeterSensor = BitConverter.ToSingle(massive, 90);

                                    //TODO: для отладки:
                                    factoryCoeffs.k_Channel0 = 1;
                                    factoryCoeffs.delta_Channel0 = 0;
                                    factoryCoeffs.k_Channel1 = 1;
                                    factoryCoeffs.delta_Channel1 = 0;
                                    factoryCoeffs.k_Channel2 = 1;
                                    factoryCoeffs.delta_Channel2 = 0;
                                    factoryCoeffs.k_Channel3 = 1;
                                    factoryCoeffs.delta_Channel3 = 0;
                                    factoryCoeffs.delta_HumiditySensor = 0;
                                    factoryCoeffs.delta_LightSensor = 0;
                                    factoryCoeffs.delta_ToutSensor = 0;
                                    Send(Proto_msgType_get_UserData());
                                    break;
                                #endregion

                                #region узнать пользовательские коэффициенты
                                case Command.get_UserData:
                                    //пользовательские калибровочные данные:
                                    //byte - step; float - k, float delta;
                                    switch (labType)
                                    {
                                        #region Phys
                                        case LabType.phys:
                                            userCoeff_Phys.step_TempSensor = massive[4];
                                            userCoeff_Phys.k_TempSensor = BitConverter.ToSingle(massive, 5);
                                            userCoeff_Phys.delta_TempSensor = BitConverter.ToSingle(massive, 9);

                                            userCoeff_Phys.step_TeslametrSensor = massive[13];
                                            userCoeff_Phys.k_TeslametrSensor = BitConverter.ToSingle(massive, 14);
                                            userCoeff_Phys.delta_TeslametrSensor = BitConverter.ToSingle(massive, 18);

                                            userCoeff_Phys.step_VoltmeterSensor = massive[22];
                                            userCoeff_Phys.k_VoltmeterSensor = BitConverter.ToSingle(massive, 23);
                                            userCoeff_Phys.delta_VoltmeterSensor = BitConverter.ToSingle(massive, 27);

                                            userCoeff_Phys.step_AmpermetrSensor = massive[31];
                                            userCoeff_Phys.k_AmpermetrSensor = BitConverter.ToSingle(massive, 32);
                                            userCoeff_Phys.delta_AmpermetrSensor = BitConverter.ToSingle(massive, 36);

                                            userCoeff_Phys.delta_AbsolutePressure = BitConverter.ToSingle(massive, 40);

                                            currentCoeff_Phys.step_TempSensor = massive[4];
                                            currentCoeff_Phys.k_TempSensor = BitConverter.ToSingle(massive, 5);
                                            currentCoeff_Phys.delta_TempSensor = BitConverter.ToSingle(massive, 9);

                                            currentCoeff_Phys.step_TeslametrSensor = massive[13];
                                            currentCoeff_Phys.k_TeslametrSensor = BitConverter.ToSingle(massive, 14);
                                            currentCoeff_Phys.delta_TeslametrSensor = BitConverter.ToSingle(massive, 18);

                                            currentCoeff_Phys.step_VoltmeterSensor = massive[22];
                                            currentCoeff_Phys.k_VoltmeterSensor = BitConverter.ToSingle(massive, 23);
                                            currentCoeff_Phys.delta_VoltmeterSensor = BitConverter.ToSingle(massive, 27);

                                            currentCoeff_Phys.step_AmpermetrSensor = massive[31];
                                            currentCoeff_Phys.k_AmpermetrSensor = BitConverter.ToSingle(massive, 32);
                                            currentCoeff_Phys.delta_AmpermetrSensor = BitConverter.ToSingle(massive, 36);

                                            currentCoeff_Phys.delta_AbsolutePressure = BitConverter.ToSingle(massive, 40);

                                            //TODO: для отладки
                                            #region
                                            userCoeff_Phys.step_TempSensor = 1;
                                            userCoeff_Phys.k_TempSensor = 1;
                                            userCoeff_Phys.delta_TempSensor = 0;

                                            userCoeff_Phys.step_TeslametrSensor = 1;
                                            userCoeff_Phys.k_TeslametrSensor = 1;
                                            userCoeff_Phys.delta_TeslametrSensor = 0;

                                            userCoeff_Phys.step_VoltmeterSensor = 1;
                                            userCoeff_Phys.k_VoltmeterSensor = 1;
                                            userCoeff_Phys.delta_VoltmeterSensor = 0;

                                            userCoeff_Phys.step_AmpermetrSensor = 1;
                                            userCoeff_Phys.k_AmpermetrSensor = 1;
                                            userCoeff_Phys.delta_AmpermetrSensor = 0;

                                            userCoeff_Phys.delta_AbsolutePressure = 0;

                                            currentCoeff_Phys.step_TempSensor = 1;
                                            currentCoeff_Phys.k_TempSensor = 1;
                                            currentCoeff_Phys.delta_TempSensor = 0;

                                            currentCoeff_Phys.step_TeslametrSensor = 1;
                                            currentCoeff_Phys.k_TeslametrSensor = 1;
                                            currentCoeff_Phys.delta_TeslametrSensor = 0;

                                            currentCoeff_Phys.step_VoltmeterSensor = 1;
                                            currentCoeff_Phys.k_VoltmeterSensor = 1;
                                            currentCoeff_Phys.delta_VoltmeterSensor = 0;

                                            currentCoeff_Phys.step_AmpermetrSensor = 1;
                                            currentCoeff_Phys.k_AmpermetrSensor = 1;
                                            currentCoeff_Phys.delta_AmpermetrSensor = 0;

                                            currentCoeff_Phys.delta_AbsolutePressure = 0;
                                            #endregion
                                            break;
                                        #endregion
                                        #region Chem
                                        case LabType.chem:
                                            userCoeff_Chem.step_pHSensor_inPH = massive[4];
                                            userCoeff_Chem.k_pHSensor_inPH = BitConverter.ToSingle(massive, 5);
                                            userCoeff_Chem.delta_pHSensor_inPH = BitConverter.ToSingle(massive, 9);

                                            userCoeff_Chem.step_TempSensor = massive[13];
                                            userCoeff_Chem.k_TempSensor = BitConverter.ToSingle(massive, 14);
                                            userCoeff_Chem.delta_TempSensor = BitConverter.ToSingle(massive, 18);

                                            userCoeff_Chem.step_ElectricalConductivitySensor = massive[22];
                                            userCoeff_Chem.k_ElectricalConductivitySensor = BitConverter.ToSingle(massive, 23);
                                            userCoeff_Chem.delta_ElectricalConductivitySensor = BitConverter.ToSingle(massive, 27);

                                            currentCoeff_Chem.step_pHSensor_inPH = massive[4];
                                            currentCoeff_Chem.k_pHSensor_inPH = BitConverter.ToSingle(massive, 5);
                                            currentCoeff_Chem.delta_pHSensor_inPH = BitConverter.ToSingle(massive, 9);

                                            currentCoeff_Chem.step_TempSensor = massive[13];
                                            currentCoeff_Chem.k_TempSensor = BitConverter.ToSingle(massive, 14);
                                            currentCoeff_Chem.delta_TempSensor = BitConverter.ToSingle(massive, 18);

                                            currentCoeff_Chem.step_ElectricalConductivitySensor = massive[22];
                                            currentCoeff_Chem.k_ElectricalConductivitySensor = BitConverter.ToSingle(massive, 23);
                                            currentCoeff_Chem.delta_ElectricalConductivitySensor = BitConverter.ToSingle(massive, 27);

                                            //TODO: для отладки
                                            #region 
                                            userCoeff_Chem.step_pHSensor_inPH = 1;
                                            userCoeff_Chem.k_pHSensor_inPH = 1;
                                            userCoeff_Chem.delta_pHSensor_inPH = 0;

                                            userCoeff_Chem.step_TempSensor = 1;
                                            userCoeff_Chem.k_TempSensor = 1;
                                            userCoeff_Chem.delta_TempSensor = 0;

                                            userCoeff_Chem.step_ElectricalConductivitySensor = 1;
                                            userCoeff_Chem.k_ElectricalConductivitySensor = 1;
                                            userCoeff_Chem.delta_ElectricalConductivitySensor = 0;

                                            currentCoeff_Chem.step_pHSensor_inPH = 1;
                                            currentCoeff_Chem.k_pHSensor_inPH = 1;
                                            currentCoeff_Chem.delta_pHSensor_inPH = 0;

                                            currentCoeff_Chem.step_TempSensor = 1;
                                            currentCoeff_Chem.k_TempSensor = 1;
                                            currentCoeff_Chem.delta_TempSensor = 0;

                                            currentCoeff_Chem.step_ElectricalConductivitySensor = 1;
                                            currentCoeff_Chem.k_ElectricalConductivitySensor = 1;
                                            currentCoeff_Chem.delta_ElectricalConductivitySensor = 0;
                                            #endregion

                                            currentStep = Step.readingData;
                                            break;
                                        #endregion
                                        case LabType.eco:
                                            break;
                                        #region BIO
                                        case LabType.bio:
                                            userCoeff_Bio.step_pHSensor_inPH = massive[4];
                                            userCoeff_Bio.k_pHSensor_inPH = BitConverter.ToSingle(massive, 5);
                                            userCoeff_Bio.delta_pHSensor_inPH = BitConverter.ToSingle(massive, 9);

                                            userCoeff_Bio.step_TempSensor = massive[13];
                                            userCoeff_Bio.k_TempSensor = BitConverter.ToSingle(massive, 14);
                                            userCoeff_Bio.delta_TempSensor = BitConverter.ToSingle(massive, 18);

                                            userCoeff_Bio.delta_ToutSensor = BitConverter.ToSingle(massive,22);
                                            userCoeff_Bio.delta_HumiditySensor = BitConverter.ToSingle(massive, 26);
                                            userCoeff_Bio.delta_LightSensor = BitConverter.ToSingle(massive, 30);

                                            currentCoeff_Bio.step_pHSensor_inPH = massive[4];
                                            currentCoeff_Bio.k_pHSensor_inPH = BitConverter.ToSingle(massive, 5);
                                            currentCoeff_Bio.delta_pHSensor_inPH = BitConverter.ToSingle(massive, 9);

                                            currentCoeff_Bio.step_TempSensor = massive[13];
                                            currentCoeff_Bio.k_TempSensor = BitConverter.ToSingle(massive, 14);
                                            currentCoeff_Bio.delta_TempSensor = BitConverter.ToSingle(massive, 18);

                                            currentCoeff_Bio.delta_ToutSensor = BitConverter.ToSingle(massive, 22);
                                            currentCoeff_Bio.delta_HumiditySensor = BitConverter.ToSingle(massive, 26);
                                            currentCoeff_Bio.delta_LightSensor = BitConverter.ToSingle(massive, 30);

                                            //TODO: для отладки
                                            #region 
                                            userCoeff_Bio.step_pHSensor_inPH = 1;
                                            userCoeff_Bio.k_pHSensor_inPH = 1;
                                            userCoeff_Bio.delta_pHSensor_inPH = 0;

                                            userCoeff_Bio.step_TempSensor = 1;
                                            userCoeff_Bio.k_TempSensor = 1;
                                            userCoeff_Bio.delta_TempSensor = 0;

                                            userCoeff_Bio.delta_ToutSensor = 0;
                                            userCoeff_Bio.delta_HumiditySensor = 0;
                                            userCoeff_Bio.delta_LightSensor = 0;

                                            currentCoeff_Bio.step_pHSensor_inPH = 1;
                                            currentCoeff_Bio.k_pHSensor_inPH = 1;
                                            currentCoeff_Bio.delta_pHSensor_inPH = 0;

                                            currentCoeff_Bio.step_TempSensor = 1;
                                            currentCoeff_Bio.k_TempSensor = 1;
                                            currentCoeff_Bio.delta_TempSensor = 0;

                                            currentCoeff_Bio.delta_ToutSensor = 0;
                                            currentCoeff_Bio.delta_HumiditySensor = 0;
                                            currentCoeff_Bio.delta_LightSensor = 0;
                                            #endregion
                                            break;
                                            #endregion
                                    }
                                    Send(DataExchange_WriteScales());
                                    break;
                                #endregion

                                #region чтение данных + смена шкал
                                case Command.data_exchange:
                                    //смена шкалы
                                    if (!dataExcange_Read)
                                    {
                                        timerMain.Start();
                                        currentStep = Step.readingData;
                                    }
                                    else
                                    {
                                        #region Сырые данные
                                        DataBoard dataBoard = new DataBoard();

                                        var state = ByteToBits(massive[4]);
                                        dataBoard.Press_ok = state[3];
                                        dataBoard.Hum_ok = state[2];
                                        dataBoard.Light_ok = state[1];
                                        dataBoard.Accel_ok = state[0];

                                        dataBoard.ConductScale = (ConductScale)massive[5];

                                        dataBoard.TempADC = BitConverter.ToInt16(massive, 12) * 0.01F;
                               
                                        dataBoard.Channel0 = BitConverter.ToSingle(massive, 16);
                                        dataBoard.Channel1 = BitConverter.ToSingle(massive, 20);
                                        dataBoard.Channel2 = BitConverter.ToSingle(massive, 24);
                                        dataBoard.Channel3 = BitConverter.ToSingle(massive, 28);

                                        dataBoard.AccelX = BitConverter.ToInt16(massive, 32);
                                        dataBoard.AccelY = BitConverter.ToInt16(massive, 34);
                                        dataBoard.AccelZ = BitConverter.ToInt16(massive, 36);
                                        dataBoard.AccelScale = (AccelScale)massive[38];

                                        dataBoard.Light = BitConverter.ToSingle(massive, 40);
                                        dataBoard.Humidity = BitConverter.ToUInt16(massive, 44) * 0.01F;

                                        dataBoard.Pressure = BitConverter.ToSingle(massive, 48);
                                        #endregion

                                        #region C учетом коэффициентов и типа лабораториит 
                                        Data data = new Data();
                                        data.Visibility = currentList.Visibility;
                                        switch (labType)
                                        {                                  
                                            #region Phys
                                            case LabType.phys:
                                                #region Accelerometr пересчет в зависимости от диапазона
                                                if (dataBoard.Accel_ok)
                                                {
                                                    switch (dataBoard.AccelScale)
                                                    {
                                                        case AccelScale.g2:
                                                            data.AccelerometerXSensor = dataBoard.AccelX * _2g_coeff;
                                                            data.AccelerometerYSensor = dataBoard.AccelY * _2g_coeff;
                                                            data.AccelerometerZSensor = dataBoard.AccelZ * _2g_coeff;
                                                            break;

                                                        case AccelScale.g4:
                                                            data.AccelerometerXSensor = dataBoard.AccelX * _4g_coeff;
                                                            data.AccelerometerYSensor = dataBoard.AccelY * _4g_coeff;
                                                            data.AccelerometerZSensor = dataBoard.AccelZ * _4g_coeff;
                                                            break;

                                                        case AccelScale.g8:
                                                            data.AccelerometerXSensor = dataBoard.AccelX * _8g_coeff;
                                                            data.AccelerometerYSensor = dataBoard.AccelY * _8g_coeff;
                                                            data.AccelerometerZSensor = dataBoard.AccelZ * _8g_coeff;
                                                            break;

                                                        case AccelScale.g16:
                                                            data.AccelerometerXSensor = dataBoard.AccelX * _16g_coeff;
                                                            data.AccelerometerYSensor = dataBoard.AccelY * _16g_coeff;
                                                            data.AccelerometerZSensor = dataBoard.AccelZ * _16g_coeff;
                                                            break;
                                                    }
                                                }
                                                #endregion

                                                //датчик давления
                                                data_withFactoryCoef.AbsolutePressureSensor = dataBoard.Pressure * 0.1F + factoryCoeffs.delta_AbsolutePressureSensor; //перевод в кПа
                                                data.AbsolutePressureSensor = data_withFactoryCoef.AbsolutePressureSensor + userCoeff_Phys.delta_AbsolutePressure;

                                                //датчик температуры - 2 канал АЦП
                                                data_withFactoryCoef.TempSensor = dataBoard.Channel2 * factoryCoeffs.k_Channel2 + factoryCoeffs.delta_Channel2;
                                                switch (userCoeff_Phys.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor + userCoeff_Phys.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor * userCoeff_Phys.k_TempSensor + userCoeff_Phys.delta_TempSensor;
                                                        break;
                                                }

                                                
                                                //тесламетр - канал 3 
                                                data_withFactoryCoef.TeslametrSensor = dataBoard.Channel3 * factoryCoeffs.k_Channel3 / 0.014F + factoryCoeffs.delta_Channel3;
                                                switch (userCoeff_Phys.step_TeslametrSensor)
                                                {
                                                    case 1:
                                                        data.TeslametrSensor = data_withFactoryCoef.TeslametrSensor + userCoeff_Phys.delta_TeslametrSensor;
                                                        break;
                                                    case 2:
                                                        data.TeslametrSensor = data_withFactoryCoef.TeslametrSensor * userCoeff_Phys.k_TeslametrSensor + userCoeff_Phys.delta_TeslametrSensor;
                                                        break;
                                                }

                                                //вольтметр - канал 0 
                                                data_withFactoryCoef.VoltmeterSensor = dataBoard.Channel0 * factoryCoeffs.k_Channel0 * 7.07F  + factoryCoeffs.delta_Channel0;
                                                switch (userCoeff_Phys.step_VoltmeterSensor)
                                                {
                                                    case 1:
                                                        data.VoltmeterSensor = data_withFactoryCoef.VoltmeterSensor + userCoeff_Phys.delta_VoltmeterSensor;
                                                        break;
                                                    case 2:
                                                        data.VoltmeterSensor = data_withFactoryCoef.VoltmeterSensor * userCoeff_Phys.k_VoltmeterSensor + userCoeff_Phys.delta_VoltmeterSensor;
                                                        break;
                                                }

                                                //амперметр - канал 1
                                                data_withFactoryCoef.AmpermetrSensor = dataBoard.Channel1 * factoryCoeffs.k_Channel1 / 1.0877F + factoryCoeffs.delta_Channel1;
                                                switch (userCoeff_Phys.step_AmpermetrSensor)
                                                {
                                                    case 1:
                                                        data.AmpermetrSensor = data_withFactoryCoef.AmpermetrSensor + userCoeff_Phys.delta_AmpermetrSensor;
                                                        break;
                                                    case 2:
                                                        data.AmpermetrSensor = data_withFactoryCoef.AmpermetrSensor * userCoeff_Phys.k_AmpermetrSensor + userCoeff_Phys.delta_AmpermetrSensor;
                                                        break;
                                                }
                                                break;
                                            #endregion

                                            #region CHEM
                                            case LabType.chem:
                                                //датчик температуры - 2 канал АЦП
                                                data_withFactoryCoef.TempSensor = dataBoard.Channel2 * factoryCoeffs.k_Channel2 + factoryCoeffs.delta_Channel2;
                                                switch (userCoeff_Chem.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor + userCoeff_Chem.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor * userCoeff_Chem.k_TempSensor + userCoeff_Chem.delta_TempSensor;
                                                        break;
                                                }

                                                //датчик рН - 0 канал АЦП
                                                data_withFactoryCoef.PhSensor_inVolt = dataBoard.Channel0 * factoryCoeffs.k_Channel0 / 3.1F + factoryCoeffs.delta_Channel0;
                                                data.PhSensor_inVolt = data_withFactoryCoef.PhSensor_inVolt;
                                                switch (userCoeff_Chem.step_pHSensor_inPH)
                                                {
                                                    case 1:
                                                        data.PhSensor_inPh = data.PhSensor_inVolt + userCoeff_Chem.delta_pHSensor_inPH;
                                                        break;
                                                    case 2:
                                                        data.PhSensor_inPh = data.PhSensor_inVolt * userCoeff_Chem.k_pHSensor_inPH + userCoeff_Chem.delta_pHSensor_inPH;
                                                        break;
                                                }
                                                if (pHSensor_mode == 0)
                                                {
                                                    data.PhSensor_Display = data.PhSensor_inVolt;
                                                    data.PhUnit_Display = "мВ";
                                                }
                                                else
                                                {
                                                    data.PhSensor_Display = data.PhSensor_inPh;
                                                    data.PhUnit_Display = "pH";
                                                }

                                                //датчик электропроводимсоти - 3 канал АЦП
                                                data_withFactoryCoef.ElectricalConductivitySensor = dataBoard.Channel3 * factoryCoeffs.k_Channel3 + factoryCoeffs.delta_Channel3;
                                                switch (userCoeff_Chem.step_ElectricalConductivitySensor)
                                                {
                                                    case 1:
                                                        data.ElectricalConductivitySensor = data_withFactoryCoef.ElectricalConductivitySensor + userCoeff_Chem.delta_ElectricalConductivitySensor;
                                                        break;
                                                    case 2:
                                                        data.ElectricalConductivitySensor = data_withFactoryCoef.ElectricalConductivitySensor * userCoeff_Chem.k_ElectricalConductivitySensor + userCoeff_Chem.delta_ElectricalConductivitySensor;
                                                        break;
                                                }
                                                break;
                                            #endregion

                                            case LabType.eco:
                                                break;

                                            #region BIO
                                            case LabType.bio:
                                                data_withFactoryCoef.HumiditySensor = dataBoard.Humidity + factoryCoeffs.delta_HumiditySensor;
                                                data.HumiditySensor = data_withFactoryCoef.HumiditySensor + userCoeff_Bio.delta_HumiditySensor;

                                                data_withFactoryCoef.LightSensor = dataBoard.Light + factoryCoeffs.delta_LightSensor;
                                                data.LightSensor = data_withFactoryCoef.LightSensor + userCoeff_Bio.delta_LightSensor;

                                                data_withFactoryCoef.TempOutsideSensor = dataBoard.TempADC + factoryCoeffs.delta_ToutSensor;
                                                data.TempOutsideSensor = data_withFactoryCoef.TempOutsideSensor + userCoeff_Bio.delta_ToutSensor;

                                                //датчик температуры - 2 канал АЦП
                                                data_withFactoryCoef.TempSensor = dataBoard.Channel2 * factoryCoeffs.k_Channel2 + factoryCoeffs.delta_Channel2;
                                                switch (userCoeff_Bio.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor + userCoeff_Bio.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor = data_withFactoryCoef.TempSensor * userCoeff_Bio.k_TempSensor + userCoeff_Bio.delta_TempSensor;
                                                        break;
                                                }


                                                //TODO:!!!!!!
                                                //датчик рН - 0 канал АЦП отображение и разные режимы
                                                data_withFactoryCoef.PhSensor_inVolt = dataBoard.Channel0 * factoryCoeffs.k_Channel0 / 3.1F + factoryCoeffs.delta_Channel0;
                                                data.PhSensor_inVolt = data_withFactoryCoef.PhSensor_inVolt;
                                                switch (userCoeff_Bio.step_pHSensor_inPH)
                                                {
                                                    case 1:
                                                        data.PhSensor_inPh = data.PhSensor_inVolt + userCoeff_Bio.delta_pHSensor_inPH;
                                                        break;
                                                    case 2:
                                                        data.PhSensor_inPh = data.PhSensor_inVolt * userCoeff_Bio.k_pHSensor_inPH + userCoeff_Bio.delta_pHSensor_inPH;
                                                        break;
                                                }
                                                if (pHSensor_mode == 0)
                                                {
                                                    data.PhSensor_Display = data.PhSensor_inVolt;
                                                    data.PhUnit_Display = "мВ";
                                                }
                                                else
                                                {
                                                    data.PhSensor_Display = data.PhSensor_inPh;
                                                    data.PhUnit_Display = "pH";
                                                }
                                                break;
                                                #endregion
                                        }

                                        #endregion

                                        #region Данные для отображения в окне калибровки
                                        switch (labType)
                                        {
                                            case LabType.phys:
                                                switch (currentCoeff_Phys.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor + currentCoeff_Phys.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor * currentCoeff_Phys.k_TempSensor + currentCoeff_Phys.delta_TempSensor;
                                                        break;
                                                }
                                                switch (currentCoeff_Phys.step_TeslametrSensor)
                                                {
                                                    case 1:
                                                        data.TeslametrSensor_Calibr = data_withFactoryCoef.TeslametrSensor + currentCoeff_Phys.delta_TeslametrSensor;
                                                        break;
                                                    case 2:
                                                        data.TeslametrSensor_Calibr = data_withFactoryCoef.TeslametrSensor * currentCoeff_Phys.k_TeslametrSensor + currentCoeff_Phys.delta_TeslametrSensor;
                                                        break;
                                                }
                                                switch (currentCoeff_Phys.step_VoltmeterSensor)
                                                {
                                                    case 1:
                                                        data.VoltmeterSensor_Calibr = data_withFactoryCoef.VoltmeterSensor + currentCoeff_Phys.delta_VoltmeterSensor;
                                                        break;
                                                    case 2:
                                                        data.VoltmeterSensor = data_withFactoryCoef.VoltmeterSensor * currentCoeff_Phys.k_VoltmeterSensor + currentCoeff_Phys.delta_VoltmeterSensor;
                                                        break;
                                                }
                                                switch (currentCoeff_Phys.step_AmpermetrSensor)
                                                {
                                                    case 1:
                                                        data.AmpermetrSensor_Calibr = data_withFactoryCoef.AmpermetrSensor + currentCoeff_Phys.delta_AmpermetrSensor;
                                                        break;
                                                    case 2:
                                                        data.AmpermetrSensor_Calibr = data_withFactoryCoef.AmpermetrSensor * currentCoeff_Phys.k_AmpermetrSensor + currentCoeff_Phys.delta_AmpermetrSensor;
                                                        break;
                                                }
                                                data.AbsolutePressureSensor_Calibr = data_withFactoryCoef.AbsolutePressureSensor + currentCoeff_Phys.delta_AbsolutePressure;
                                                break;

                                            case LabType.chem:
                                                switch (currentCoeff_Chem.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor + currentCoeff_Chem.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor * currentCoeff_Chem.k_TempSensor + currentCoeff_Chem.delta_TempSensor;
                                                        break;
                                                }
                                                switch (currentCoeff_Chem.step_pHSensor_inPH)
                                                {
                                                    case 1:
                                                        data.PhSensor_inPh_Calibr = data.PhSensor_inVolt + currentCoeff_Chem.delta_pHSensor_inPH;
                                                        break;
                                                    case 2:
                                                        data.PhSensor_inPh_Calibr = data.PhSensor_inVolt * currentCoeff_Chem.k_pHSensor_inPH + currentCoeff_Chem.delta_pHSensor_inPH;
                                                        break;
                                                }
                                                switch (currentCoeff_Chem.step_ElectricalConductivitySensor)
                                                {
                                                    case 1:
                                                        data.ElectricalConductivitySensor_Calibr = data_withFactoryCoef.ElectricalConductivitySensor + currentCoeff_Chem.delta_ElectricalConductivitySensor;
                                                        break;
                                                    case 2:
                                                        data.ElectricalConductivitySensor_Calibr = data_withFactoryCoef.ElectricalConductivitySensor * currentCoeff_Chem.k_ElectricalConductivitySensor + currentCoeff_Chem.delta_ElectricalConductivitySensor;
                                                        break;
                                                }
                                                break;

                                            case LabType.eco:
                                                break;

                                            case LabType.bio:
                                                data.HumiditySensor_Calibr = data_withFactoryCoef.HumiditySensor + currentCoeff_Bio.delta_HumiditySensor;
                                                data.LightSensor_Calibr = data_withFactoryCoef.LightSensor + currentCoeff_Bio.delta_LightSensor;
                                                data.TempOutsideSensor_Calibr = data_withFactoryCoef.TempOutsideSensor + currentCoeff_Bio.delta_ToutSensor;
                                                switch (currentCoeff_Bio.step_TempSensor)
                                                {
                                                    case 1:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor + currentCoeff_Bio.delta_TempSensor;
                                                        break;
                                                    case 2:
                                                        data.TempSensor_Calibr = data_withFactoryCoef.TempSensor * currentCoeff_Bio.k_TempSensor + currentCoeff_Bio.delta_TempSensor;
                                                        break;
                                                }
                                                switch (currentCoeff_Bio.step_pHSensor_inPH)
                                                {
                                                    case 1:
                                                        data.PhSensor_inPh_Calibr = data.PhSensor_inVolt + currentCoeff_Bio.delta_pHSensor_inPH;
                                                        break;
                                                    case 2:
                                                        data.PhSensor_inPh_Calibr = data.PhSensor_inVolt * currentCoeff_Bio.k_pHSensor_inPH + currentCoeff_Bio.delta_pHSensor_inPH;
                                                        break;
                                                }
                                                break;
                                        }
                                        #endregion

                                        currentData = data;
                                        DataUpdate?.Invoke(this, data);
                                        //TODO: сообщение о неисправности цифровых датчиков!
                                    }
                                    break;
                                #endregion

                                //пользовательские коэффициенты успешно записаны
                                case Command.set_UserData:
                                    if (massive[3] == set_UserData)
                                        currentStep = Step.readingData;
                                    break;

                                #region Запуск архива либо информация об архиве
                                case Command.archive_Control:
                                    //чтение о статусе текущего архива
                                    if(archiveInfo)
                                    {
                                        archiveSettingsFromDevice = new ArchiveSettings();
                                        archiveSettingsFromDevice.RecordPeriod = BitConverter.ToUInt32(massive, 4);
                                        archiveSettingsFromDevice.RecordNumber = massive[8];
                                        archiveSettingsFromDevice.ArchiveStatus = massive[9];
                                        archiveSettingsFromDevice.StartRecord_year = (byte)(2000 + massive[11]);
                                        archiveSettingsFromDevice.StartRecord_month = massive[12];
                                        archiveSettingsFromDevice.StartRecord_day = massive[13];
                                        archiveSettingsFromDevice.StartRecord_hour = massive[14];
                                        archiveSettingsFromDevice.StartRecord_min = massive[15];
                                        archiveSettingsFromDevice.StartRecord_sec = massive[16];

                                        numberOfArchiveRecords = archiveSettingsFromDevice.RecordNumber = massive[8];

                                        string status = "";
                                        bool buttonEnable = false;
                                        switch (archiveSettingsFromDevice.ArchiveStatus)
                                        {
                                            //архив неинициализирован, данные архива некорректны
                                            case 0:
                                                status = "архив пустой";
                                                buttonEnable = false;
                                                break;

                                            //идет процесс сохранения данных
                                            case 1:
                                                status = "идет сбор архива";
                                                buttonEnable = false;
                                                break;

                                            //архив сохранен, идет загрузка архива из энергонезависимой памяти. Подождите
                                            case 2:
                                                status = "идет сбор архива";
                                                buttonEnable = false;
                                                break;

                                            //архив сохранен, может быть прочитан
                                            case 3:
                                                status = "архив готов";
                                                buttonEnable = true;
                                                break;
                                        }

                                        ArchiveInfoUpdate?.Invoke(this, new ArchiveInfoUpdateArgs(status, buttonEnable, massive[9]));
                                    }

                                    //запуск архива успешно
                                    else
                                    {
                                        currentStep = Step.archiveStatus;
                                    }
                                    break;
                                #endregion

                                case Command.getArchive:
                                    #region Архив
                                    for (int l = 0; l < 4; l++)
                                    {
                                        DataBoard archiveData = new DataBoard();

                                        var stateArchive = ByteToBits(massive[6 + 74 * l]);
                                        archiveData.Press_ok = stateArchive[3];
                                        archiveData.Hum_ok = stateArchive[2];
                                        archiveData.Light_ok = stateArchive[1];
                                        archiveData.Accel_ok = stateArchive[0];

                                        archiveData.ConductScale = (ConductScale)massive[7 + 74 * l];

                                        archiveData.TempADC = BitConverter.ToInt16(massive, 14 + 74 * l) * 0.01F;

                                        archiveData.Channel0 = BitConverter.ToSingle(massive, 18 + 74 * l);
                                        archiveData.Channel1 = BitConverter.ToSingle(massive, 22 + 74 * l);
                                        archiveData.Channel2 = BitConverter.ToSingle(massive, 26 + 74 * l);
                                        archiveData.Channel3 = BitConverter.ToSingle(massive, 30 + 74 * l);

                                        archiveData.AccelX = BitConverter.ToInt16(massive, 34 + 74 * l);
                                        archiveData.AccelY = BitConverter.ToInt16(massive, 36 + 74 * l);
                                        archiveData.AccelZ = BitConverter.ToInt16(massive, 38 + 74 * l);
                                        archiveData.AccelScale = (AccelScale)massive[40 + 74 * l];

                                        archiveData.Light = BitConverter.ToSingle(massive, 42 + 74 * l);
                                        archiveData.Humidity = BitConverter.ToUInt16(massive, 46 + 74 * l) * 0.01F;

                                        archiveData.Pressure = BitConverter.ToSingle(massive, 50 + 74 * l);

                                        archiveDataFromDevice.Add(archiveData);
                                    }
                                    if (archiveCurrentStep == archiveNeedSteps && intArchiveRecord)
                                    {
                                        currentStep = Step.archiveStatus;
                                        var fff = archiveDataFromDevice;
                                    }



                                    #endregion
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            return success;
        }

        #region Архивы
        public void Archive_Status(bool read)
        {
            if (read)
            {
                archiveInfo = true;
                currentStep = Step.archiveStatus;
            } 
            else currentStep = Step.readingData;
        }
        public void Archive_Start(uint Period, byte Number)
        {
            archiveInfo = false;          
            archiveToStart = new ArchiveSettings(Period, Number);
            currentStep = Step.setArchive;
        }

        public void Archive_Read()
        {
            currentStep = Step.getArchive;
            intArchiveRecord = (numberOfArchiveRecords % 4 == 0);
            archiveNeedSteps = numberOfArchiveRecords / 4;
            archiveCurrentStep = 0;
            archiveDataFromDevice = new List<DataBoard>();
        }
        #endregion

        public void Change_Accel_Scale(AccelScale scale)
        {
            currentAccelScale = scale;
            currentStep = Step.writeScale;
        }
        public void Change_ElectricalConductivity_Scale(ConductScale scale)
        {
            currentConductScale = scale;
            currentStep = Step.writeScale;
        }

        #region Протокольная часть
        private byte[] Proto_msgType_getInfo()
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
        
        //Чтение данных
        private byte[] DataExchange_Read()
        {
            dataExcange_Read = true;

            currentCommand = (Command)data_exchange;

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

        //изменение шкал
        private byte[] DataExchange_WriteScales()
        {
            dataExcange_Read = false;

            currentCommand = (Command)data_exchange;

            byte[] writeMessage = new byte[8];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = data_exchange;

            writeMessage[4] = (byte)currentAccelScale;
            writeMessage[5] = (byte)currentConductScale;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[6] = crc[0];
            writeMessage[7] = crc[1];

            return writeMessage;
        }

        //Чтение заводских настроек
        private byte[] Get_HW_Config()
        {
            currentCommand = (Command)get_HW_Config;

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
        
        //запись пользовательских настроек
        private byte[] Proto_msgType_set_UserData()
        {
            currentCommand = (Command)set_UserData;

            byte[] writeMessage = new byte[262];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = set_UserData;

            switch(labType)
            {
                #region PHYS
                case LabType.phys:
                    writeMessage[4] = (byte)userCoeff_Phys.step_TempSensor;
                    var kBytes_TempSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.k_TempSensor);
                    writeMessage[5] = kBytes_TempSensor_Phys[0];
                    writeMessage[6] = kBytes_TempSensor_Phys[1];
                    writeMessage[7] = kBytes_TempSensor_Phys[2];
                    writeMessage[8] = kBytes_TempSensor_Phys[3];

                    var deltaBytes_TempSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.delta_TempSensor);
                    writeMessage[9] = deltaBytes_TempSensor_Phys[0];
                    writeMessage[10] = deltaBytes_TempSensor_Phys[1];
                    writeMessage[11] = deltaBytes_TempSensor_Phys[2];
                    writeMessage[12] = deltaBytes_TempSensor_Phys[3];

                    writeMessage[13] = (byte)userCoeff_Phys.step_TeslametrSensor;
                    var kBytes_TeslametrSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.k_TeslametrSensor);
                    writeMessage[14] = kBytes_TeslametrSensor_Phys[0];
                    writeMessage[15] = kBytes_TeslametrSensor_Phys[1];
                    writeMessage[16] = kBytes_TeslametrSensor_Phys[2];
                    writeMessage[17] = kBytes_TeslametrSensor_Phys[3];

                    var deltaBytes_TeslametrSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.delta_TeslametrSensor);
                    writeMessage[18] = deltaBytes_TeslametrSensor_Phys[0];
                    writeMessage[19] = deltaBytes_TeslametrSensor_Phys[1];
                    writeMessage[20] = deltaBytes_TeslametrSensor_Phys[2];
                    writeMessage[21] = deltaBytes_TeslametrSensor_Phys[3];

                    writeMessage[22] = (byte)userCoeff_Phys.step_VoltmeterSensor;
                    var kBytes_VoltmeterSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.k_VoltmeterSensor);
                    writeMessage[23] = kBytes_VoltmeterSensor_Phys[0];
                    writeMessage[24] = kBytes_VoltmeterSensor_Phys[1];
                    writeMessage[25] = kBytes_VoltmeterSensor_Phys[2];
                    writeMessage[26] = kBytes_VoltmeterSensor_Phys[3];

                    var deltaBytes_VoltmeterSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.delta_VoltmeterSensor);
                    writeMessage[27] = deltaBytes_VoltmeterSensor_Phys[0];
                    writeMessage[28] = deltaBytes_VoltmeterSensor_Phys[1];
                    writeMessage[29] = deltaBytes_VoltmeterSensor_Phys[2];
                    writeMessage[30] = deltaBytes_VoltmeterSensor_Phys[3];

                    writeMessage[31] = (byte)userCoeff_Phys.step_AmpermetrSensor;
                    var kBytes_AmpermetrSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.k_AmpermetrSensor);
                    writeMessage[32] = kBytes_AmpermetrSensor_Phys[0];
                    writeMessage[33] = kBytes_AmpermetrSensor_Phys[1];
                    writeMessage[34] = kBytes_AmpermetrSensor_Phys[2];
                    writeMessage[35] = kBytes_AmpermetrSensor_Phys[3];

                    var deltaBytes_AmpermetrSensor_Phys = BitConverter.GetBytes(userCoeff_Phys.delta_AmpermetrSensor);
                    writeMessage[36] = deltaBytes_AmpermetrSensor_Phys[0];
                    writeMessage[37] = deltaBytes_AmpermetrSensor_Phys[1];
                    writeMessage[38] = deltaBytes_AmpermetrSensor_Phys[2];
                    writeMessage[39] = deltaBytes_AmpermetrSensor_Phys[3];

                    var deltaBytes_AbsPressureSensor = BitConverter.GetBytes(userCoeff_Phys.delta_AbsolutePressure);
                    writeMessage[40] = deltaBytes_AbsPressureSensor[0];
                    writeMessage[41] = deltaBytes_AbsPressureSensor[1];
                    writeMessage[42] = deltaBytes_AbsPressureSensor[2];
                    writeMessage[43] = deltaBytes_AbsPressureSensor[3];
                    break;
                #endregion

                #region CHEM
                case LabType.chem:
                    writeMessage[4] = (byte)userCoeff_Chem.step_pHSensor_inPH;
                    var kBytes_pHSensor_chem = BitConverter.GetBytes(userCoeff_Chem.k_pHSensor_inPH);
                    writeMessage[5] = kBytes_pHSensor_chem[0];
                    writeMessage[6] = kBytes_pHSensor_chem[1];
                    writeMessage[7] = kBytes_pHSensor_chem[2];
                    writeMessage[8] = kBytes_pHSensor_chem[3];

                    var deltaBytes_pHSensor_chem = BitConverter.GetBytes(userCoeff_Chem.delta_pHSensor_inPH);
                    writeMessage[9] = deltaBytes_pHSensor_chem[0];
                    writeMessage[10] = deltaBytes_pHSensor_chem[1];
                    writeMessage[11] = deltaBytes_pHSensor_chem[2];
                    writeMessage[12] = deltaBytes_pHSensor_chem[3];

                    writeMessage[13] = (byte)userCoeff_Chem.step_TempSensor;
                    var kBytes_TempSensor_chem = BitConverter.GetBytes(userCoeff_Chem.k_TempSensor);
                    writeMessage[14] = kBytes_TempSensor_chem[0];
                    writeMessage[15] = kBytes_TempSensor_chem[1];
                    writeMessage[16] = kBytes_TempSensor_chem[2];
                    writeMessage[17] = kBytes_TempSensor_chem[3];

                    var deltaBytes_TempSensor_chem = BitConverter.GetBytes(userCoeff_Chem.delta_TempSensor);
                    writeMessage[18] = deltaBytes_TempSensor_chem[0];
                    writeMessage[19] = deltaBytes_TempSensor_chem[1];
                    writeMessage[20] = deltaBytes_TempSensor_chem[2];
                    writeMessage[21] = deltaBytes_TempSensor_chem[3];

                    writeMessage[22] = (byte)userCoeff_Chem.step_ElectricalConductivitySensor;
                    var kBytes_ElectricalConductivitySensor_chem = BitConverter.GetBytes(userCoeff_Chem.k_ElectricalConductivitySensor);
                    writeMessage[23] = kBytes_TempSensor_chem[0];
                    writeMessage[24] = kBytes_TempSensor_chem[1];
                    writeMessage[25] = kBytes_TempSensor_chem[2];
                    writeMessage[26] = kBytes_TempSensor_chem[3];

                    var deltaBytes_ElectricalConductivitySensor_chem = BitConverter.GetBytes(userCoeff_Chem.delta_ElectricalConductivitySensor);
                    writeMessage[27] = deltaBytes_ElectricalConductivitySensor_chem[0];
                    writeMessage[28] = deltaBytes_ElectricalConductivitySensor_chem[1];
                    writeMessage[29] = deltaBytes_ElectricalConductivitySensor_chem[2];
                    writeMessage[30] = deltaBytes_ElectricalConductivitySensor_chem[3];
                    break;
                #endregion

                case LabType.eco:
                    break;

                #region BIO
                case LabType.bio:
                    writeMessage[4] = (byte)userCoeff_Bio.step_pHSensor_inPH;
                    var kBytes_pHSensor_bio = BitConverter.GetBytes(userCoeff_Bio.k_pHSensor_inPH);
                    writeMessage[5] = kBytes_pHSensor_bio[0];
                    writeMessage[6] = kBytes_pHSensor_bio[1];
                    writeMessage[7] = kBytes_pHSensor_bio[2];
                    writeMessage[8] = kBytes_pHSensor_bio[3];

                    var deltaBytes_pHSensor_bio = BitConverter.GetBytes(userCoeff_Bio.delta_pHSensor_inPH);
                    writeMessage[9] = deltaBytes_pHSensor_bio[0];
                    writeMessage[10] = deltaBytes_pHSensor_bio[1];
                    writeMessage[11] = deltaBytes_pHSensor_bio[2];
                    writeMessage[12] = deltaBytes_pHSensor_bio[3];

                    writeMessage[13] = (byte)userCoeff_Bio.step_TempSensor;
                    var kBytes_TempSensor_bio = BitConverter.GetBytes(userCoeff_Bio.k_TempSensor);
                    writeMessage[14] = kBytes_TempSensor_bio[0];
                    writeMessage[15] = kBytes_TempSensor_bio[1];
                    writeMessage[16] = kBytes_TempSensor_bio[2];
                    writeMessage[17] = kBytes_TempSensor_bio[3];

                    var deltaBytes_TempSensor_bio = BitConverter.GetBytes(userCoeff_Bio.delta_TempSensor);
                    writeMessage[18] = deltaBytes_TempSensor_bio[0];
                    writeMessage[19] = deltaBytes_TempSensor_bio[1];
                    writeMessage[20] = deltaBytes_TempSensor_bio[2];
                    writeMessage[21] = deltaBytes_TempSensor_bio[3];

                    var deltaBytes_ToutSensor_bio = BitConverter.GetBytes(userCoeff_Bio.delta_ToutSensor);
                    writeMessage[22] = deltaBytes_ToutSensor_bio[0];
                    writeMessage[23] = deltaBytes_ToutSensor_bio[1];
                    writeMessage[24] = deltaBytes_ToutSensor_bio[2];
                    writeMessage[25] = deltaBytes_ToutSensor_bio[3];

                    var deltaBytes_HumiditySensor_bio = BitConverter.GetBytes(userCoeff_Bio.delta_HumiditySensor);
                    writeMessage[26] = deltaBytes_HumiditySensor_bio[0];
                    writeMessage[27] = deltaBytes_HumiditySensor_bio[1];
                    writeMessage[28] = deltaBytes_HumiditySensor_bio[2];
                    writeMessage[29] = deltaBytes_HumiditySensor_bio[3];

                    var deltaBytes_LightSensor_bio = BitConverter.GetBytes(userCoeff_Bio.delta_LightSensor);
                    writeMessage[30] = deltaBytes_LightSensor_bio[0];
                    writeMessage[31] = deltaBytes_LightSensor_bio[1];
                    writeMessage[32] = deltaBytes_LightSensor_bio[2];
                    writeMessage[33] = deltaBytes_LightSensor_bio[3];
                    break;
                    #endregion
            }

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[260] = crc[0];
            writeMessage[261] = crc[1];

            return writeMessage;
        }

        //чтение пользовательских настроек
        private byte[] Proto_msgType_get_UserData()
        {
            currentCommand = (Command)get_UserData;

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

        //запрос информации текущего архива
        private byte[] ArchiveControl_Info()
        {
            currentCommand = (Command)archive_Control;

            byte[] writeMessage = new byte[6];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = archive_Control;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[4] = crc[0];
            writeMessage[5] = crc[1];

            return writeMessage;
        }

        //запуск режима архива
        private byte[] ArchiveControl_Start(ArchiveSettings archive)
        {
            currentCommand = (Command)archive_Control;

            byte[] writeMessage = new byte[36];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = archive_Control;

            
            writeMessage[4] = BitConverter.GetBytes(archive.RecordPeriod)[0];
            writeMessage[5] = BitConverter.GetBytes(archive.RecordPeriod)[1];
            writeMessage[6] = BitConverter.GetBytes(archive.RecordPeriod)[2];
            writeMessage[7] = BitConverter.GetBytes(archive.RecordPeriod)[3];
            writeMessage[8] = archive.RecordNumber;
            writeMessage[11] = archive.StartRecord_year;
            writeMessage[12] = archive.StartRecord_month;
            writeMessage[13] = archive.StartRecord_day;
            writeMessage[14] = archive.StartRecord_hour;
            writeMessage[15] = archive.StartRecord_min;
            writeMessage[16] = archive.StartRecord_sec;
            
            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[34] = crc[0];
            writeMessage[35] = crc[1];

            return writeMessage;
        }

        //чтение архива
        private byte[] Archive_Get(byte startAddr, byte recordQtt)
        {
            currentCommand = (Command)Command.getArchive;

            byte[] writeMessage = new byte[8];
            writeMessage[0] = startByte;
            writeMessage[1] = BitConverter.GetBytes(writeMessage.Length)[0];
            writeMessage[2] = BitConverter.GetBytes(writeMessage.Length)[1];
            writeMessage[3] = getArchive;

            writeMessage[4] = startAddr;
            writeMessage[5] = recordQtt;

            var crc = CalculateCrc16Modbus(writeMessage);
            writeMessage[6] = crc[0];
            writeMessage[7] = crc[1];

            return writeMessage;
        }

        #endregion

        #region CRC
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

        private bool[] ByteToBits(byte bt)
        {
            bool[] bits = new bool[8];
            for (int i = 0; i < 8; i++)
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
    }
}
