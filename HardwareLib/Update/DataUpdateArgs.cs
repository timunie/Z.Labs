using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib
{
    public class DataUpdateArgs : EventArgs
    {
        public Data Data { get; private set; }

        public DataUpdateArgs(Data Data)
        {
            this.Data = Data;
        }
    }
}
