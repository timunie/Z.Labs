using HardwareLib.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class SensorListUpdateArgs : EventArgs
    {
        public SensorList SensorList { get; private set; }

        public SensorListUpdateArgs(SensorList SensorList)
        {
            this.SensorList = SensorList;
        }
    }
}
