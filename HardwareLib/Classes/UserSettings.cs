using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareLib
{
    public class UserSettings
    {
        public UserSettings() { }
        public byte stepsCount { get; set; }
        public float k { get; set; }
        public float delta { get; set; }

    }
}
