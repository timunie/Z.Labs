using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class DataBoardUpdateArgs : EventArgs
    {
        public DataBoard Data { get; private set; }

        public DataBoardUpdateArgs(DataBoard Data)
        {
            this.Data = Data;
        }
    }
}
