using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Update
{
    public class ArchiveInfoUpdateArgs : EventArgs
    {
        public string ArchiveStatus { get; set; }
        public bool EnableBut { get; set; }
        public byte ArchiveStatus_Val { get; set; }
        public ArchiveInfoUpdateArgs(string ArchiveStatus, bool EnableBut, byte ArchiveStatus_Val)
        {
            this.ArchiveStatus = ArchiveStatus;
            this.EnableBut = EnableBut;
            this.ArchiveStatus_Val = ArchiveStatus_Val;
        }
    }
}
