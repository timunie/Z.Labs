using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;

namespace ZLabs.Models;

public class Sensor
{
    public Sensor()
    {
        AddDefaultSettings();
    }

    protected void AddDefaultSettings()
    {
        var periodComboBox = new ComboBox()
        {
            Items = new[]
            {
                "4 точек/сек.",
                "2 точек/сек.",
                "1 точка/сек.",
                "1 точка/мин.",
                "1 точка/10 мин.",
                "1 точка/15 мин.",
                "1 точка/30 мин.",
                "1 точка/час"
            }
        };
        periodComboBox.SelectedIndex = 0;

        var graphLineColorPicker = new AvaloniaColorPicker.ColorButton();
        graphLineColorPicker.Color = Colors.Red;
        
        var graphPotsColorPicker = new AvaloniaColorPicker.ColorButton();
        graphPotsColorPicker.Color = Colors.Red;
        
        var comboBox1 = new ComboBox {Items = Enumerable.Range(2, 8)};
        comboBox1.SelectedIndex = 0;
        
        var comboBox2 = new ComboBox {Items = Enumerable.Range(2, 8)};
        comboBox2.SelectedIndex = 0;

        var settings = new SensorSetting[]
        {
            new("Период опроса", periodComboBox),
            new("Цвет линии графика", graphLineColorPicker),
            new("Толщина линии графика", comboBox1),
            new("Цвет точек графика", graphPotsColorPicker),
            new("Величина точек графика", comboBox2)
        };
      
        Settings = new ObservableCollection<SensorSetting>(settings);
    }

    public string Name { get; set; }
    public string ImagePath { get; set; }
    public ObservableCollection<SensorSetting> Settings { get; set; } = new();
}

public class SensorSetting
{
    public SensorSetting(string label, Control selector)
    {
        Label = label;
        Selector = selector;
    }

    public string Label { get; set; }
    public Control Selector { get; set; }
}