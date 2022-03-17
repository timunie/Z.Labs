using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib.Classes
{
    public class ArchiveSettings
    {
        public ArchiveSettings()
        {

        }
        public ArchiveSettings(uint Period, byte Number)
        {
            RecordPeriod = Period;
            RecordNumber = Number;
            DateTime date = DateTime.Now;
            StartRecord_year = (byte)(date.Year - 2000);
            StartRecord_month = (byte)date.Month;
            StartRecord_day = (byte)date.Day;
            StartRecord_hour = (byte)date.Hour;
            StartRecord_min = (byte)date.Minute;
            StartRecord_sec = (byte)date.Second;
        }
        public uint RecordPeriod { get; set; }
        public byte RecordNumber { get;  set; }
        public byte StartRecord_year { get; set; }
        public byte StartRecord_month { get; set; }
        public byte StartRecord_day { get; set; }
        public byte StartRecord_hour { get; set; }
        public byte StartRecord_min { get; set; }
        public byte StartRecord_sec { get; set; }

        public byte ArchiveStatus { get; set; }
    }
}
