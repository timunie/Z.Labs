using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class EventsUpdateArgs : EventArgs
    {
        public string Message { get; set; }
        public bool Error { get; set; }
        public EventsUpdateArgs(string Message, bool Error)
        {
            this.Message = Message;
            this.Error = Error;
        }
    }
}
