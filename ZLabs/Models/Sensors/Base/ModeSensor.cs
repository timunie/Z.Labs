using System.Collections.Generic;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace ZLabs.Models.Implementations;

public class ModeSensor : Sensor
{
    public string Mode { get; private set; }
    
    public ModeSensor(string name, string imagePath, IList<string> modes) : base(name, imagePath)
    {
        var modeComboBox = new ComboBox()
        {
            Items = modes
        };
        modeComboBox.SelectionChanged += (_, _) =>
        {
            Mode = modes[modeComboBox.SelectedIndex];
        };
        modeComboBox.SelectedIndex = 0;
        
        Settings.Insert(0, new SensorSetting("Режим измерения", modeComboBox));
    }
}