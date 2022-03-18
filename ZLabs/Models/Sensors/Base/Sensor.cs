using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaColorPicker;
using ScottPlot.Avalonia;

namespace ZLabs.Models;

public class Sensor : IPage
{
    public readonly AvaPlot Plot = new AvaPlot();

    public string Name { get; set; }
    public string ImagePath { get; set; }
    public ObservableCollection<SensorSetting> Settings { get; set; } = new();

    protected readonly DispatcherTimer _timer = new();
    protected int _markerSize = 2;
    protected Color _markerColor = Colors.Red;
    protected int _lineSize = 2;
    protected Color _lineColor = Colors.Red;
    protected PlotRange? _plotRange;

    public Sensor(string name, string imagePath)
    {
        Name = name;
        ImagePath = imagePath;
        AddDefaultSettings();
    }

    private static readonly Dictionary<string, int> periods = new()
    {
        {"4 точек/сек.", 250},
        {"2 точек/сек.", 500},
        {"1 точка/сек.", 1000},
        {"1 точка/мин.", 60000},
        {"1 точка/10 мин.", 600000},
        {"1 точка/15 мин.", 900000},
        {"1 точка/30 мин.", 1800000},
        {"1 точка/час", 3600000},
    };

    protected void AddDefaultSettings()
    {
        var periodComboBox = new ComboBox()
        {
            Items = periods.Keys
        };
        periodComboBox.SelectionChanged += (_, _) =>
        {
            var period = periods.ElementAt(periodComboBox.SelectedIndex).Value;
            _timer.Interval = TimeSpan.FromMilliseconds(period);
        };
        periodComboBox.SelectedIndex = 0;

        var graphLineColorPicker = new ColorButton();
        graphLineColorPicker.PropertyChanged += (_, change) =>
        {
            if (!(change.Property == ColorButton.ColorProperty))
                return;
            _lineColor = change.NewValue is Color color ? color : Colors.Red;
        };
        graphLineColorPicker.Color = Colors.Red;


        var graphMarkerColorPicker = new ColorButton();
        graphMarkerColorPicker.PropertyChanged += (_, change) =>
        {
            if (!(change.Property == ColorButton.ColorProperty))
                return;
            _markerColor = change.NewValue is Color color ? color : Colors.Red;
        };
        graphMarkerColorPicker.Color = Colors.Red;

        var lineSizeComboBox = new ComboBox {Items = Enumerable.Range(2, 8)};
        lineSizeComboBox.SelectionChanged += (_, _) => { _lineSize = (int) (lineSizeComboBox.SelectedItem ?? 2); };
        lineSizeComboBox.SelectedIndex = 0;

        var markerSizeComboBox = new ComboBox {Items = Enumerable.Range(2, 8)};
        markerSizeComboBox.SelectionChanged += (_, _) =>
        {
            _markerSize = (int) (markerSizeComboBox.SelectedItem ?? 2);
        };
        markerSizeComboBox.SelectedIndex = 0;

        var settings = new SensorSetting[]
        {
            new("Период опроса", periodComboBox),
            new("Цвет линии графика", graphLineColorPicker),
            new("Толщина линии графика", lineSizeComboBox),
            new("Цвет точек графика", graphMarkerColorPicker),
            new("Величина точек графика", markerSizeComboBox)
        };

        Settings = new ObservableCollection<SensorSetting>(settings);
    }


    private SensorSetting? _rangeUnitSetting;
    public Sensor AddRanges(ICollection<PlotRange?> ranges, int startIndex = 0,
        int positionIndex = 0)
    {
        if (_rangeUnitSetting != null)
        {
            Settings.Remove(_rangeUnitSetting);
            _rangeUnitSetting = null;
        }
        var rangeUnitComboBox = new ComboBox
        {
            Items = ranges.Select(range => PlotRange.ToString(range))
        };
        rangeUnitComboBox.SelectionChanged += (_, _) =>
        {
            _plotRange = ranges.ElementAt(rangeUnitComboBox.SelectedIndex);
        };
        
        rangeUnitComboBox.SelectedIndex = Math.Clamp(startIndex, 0, ranges.Count - 1);
        positionIndex = Math.Clamp(positionIndex, 0, ranges.Count - 1);

        _rangeUnitSetting = new SensorSetting("Диапазон / Ед. Измерения", rangeUnitComboBox);
        Settings.Insert(positionIndex, _rangeUnitSetting);
        
        return this;
    }
}

public struct PlotRange
{
    public double Min;
    public double Max;
    public string Unit;
    private Func<double, double> Converter;

    public PlotRange(double min, double max, string unit, double unitMultiplier = 1,
        Func<double, double>? converter = null)
    {
        Min = min;
        Max = max;
        Unit = unit;
        
        converter ??= d => d * unitMultiplier;
        Converter = converter;
    }

    public double Convert(double val) => Converter(val);

    public static string ToString(PlotRange? nullableRange)
    {
        if (nullableRange is { } range)
            return $"{range.Min}...+{range.Max} {range.Unit}";
        return "отключен";
    }
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