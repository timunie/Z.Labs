using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaColorPicker;
using ScottPlot.Avalonia;
using ZLabs.Helpers;

namespace ZLabs.Models;

public class Sensor : IPage
{

    /// <summary> Отображаемое имя датчика </summary>
    public string Name { get; }

    /// <summary> Путь до картинки в формате "/Assets/Path/To/Image.png" </summary>
    public string ImagePath { get; }

    public Bitmap? Image => BitmapAssetValueConverter.Instance.Convert(ImagePath);
    
    /// <summary> Набор настроек датчика и его графика </summary>
    public ObservableCollection<SensorSetting> Settings { get; set; } = new();

    /// <summary> Фугкция применяющая настройки кабибровки. Если Null то игнорируется </summary>
    public Func<double, double>? Calibrator;

    /// <summary> Находится ли датчик в связке датчиков </summary>
    public bool InStack { get; set; } = false;

    /// <summary> Таймер опроса датчика </summary>
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

    public void Calibrate(Func<double, double>? calibrator)
    {
        Calibrator = calibrator;
    }

    private double GetCurrentValue()
    {
        var testValue = 30d + Random.Shared.NextDouble();
        var result = testValue;

        if (Calibrator != null)
            result = Calibrator(result);

        if (_plotRange != null)
            result = _plotRange.Value.Convert(result);
        
        return result;
    }

    private static readonly Dictionary<string, int> Periods = new()
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

    // Стандартные настройки для графика
    protected void AddDefaultSettings()
    {
        var periodComboBox = new ComboBox()
        {
            Items = Periods.Keys
        };
        periodComboBox.SelectionChanged += (_, _) =>
        {
            var period = Periods.ElementAt(periodComboBox.SelectedIndex).Value;
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


    // Функционал добавления выбора диапазона отображения графиков 
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

// Диапазон отображаемых данных на графике 
public struct PlotRange
{
    // Минимальное значениче в указанных ед. измерения
    public double Min;
    // Максимсальное значениче в указанных ед. измерения
    public double Max;
    // отображаемая ед. измерения
    public string Unit;
    // Функция конвертации едениц измерения
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

    // Функция конвертации едениц измерения
    public double Convert(double val) => Converter(val);

    // Строковое представление диапазона с защитой от Null
    public static string ToString(PlotRange? nullableRange)
    {
        if (nullableRange is { } range)
            return $"{range.Min}...+{range.Max} {range.Unit}";
        return "отключен";
    }
}

// Одна настройка датчика (нужно для представления, не хранит данные)
public class SensorSetting
{
    public SensorSetting(string label, Control selector)
    {
        Label = label;
        Selector = selector;
    }
    
    // Наименование настройки
    public string Label { get; set; }
    // Представление настройки (как ее можно изменить)
    public Control Selector { get; set; }
}