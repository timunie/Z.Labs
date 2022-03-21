using System.Linq;
using ZLabs.Models;

namespace ZLabs.ViewModels;

public class SettingsViewModel : IPage
{
   public string Name => "Настройки";
   public string ImagePath => "/Assets/Img/Gear.png";

    public ChartFormatListBoxItem SelectedChartFormatItem
    {
        get => ChartFormats.First(item => item.Format == Settings.ChartFormat);
        set => Settings.ChartFormat = value.Format;
    }

    public TimeFormatListBoxItem SelectedTimeFormatItem
    {
        get => TimeFormats.First(item => item.Format == Settings.TimeFormat);
        set => Settings.TimeFormat = value.Format;
    }

    public TimeUnit SelectedTimeUnit
    {
        get => Settings.TimeUnit;
        set => Settings.TimeUnit = value;
    }
    
    public double ExperimentTime
    {
        get => Settings.ExperimentTime;
        set => Settings.ExperimentTime = value;
    }

    public TimeFormatListBoxItem[] TimeFormats { get; }
    public ChartFormatListBoxItem[] ChartFormats { get; }
    public TimeUnit[] TimeUnits => TimeUnit.TimeUnits;

    public SettingsViewModel()
    {
        TimeFormats = new TimeFormatListBoxItem[]
        {
            new("Секундомер", TimeFormat.Stopwatch),
            new("ММ:СС", TimeFormat.MinutesSeconds),
            new("ЧЧ:ММ", TimeFormat.HoursMinutes),
        };

        ChartFormats = new ChartFormatListBoxItem[]
        {
            new("Линия и точки", ChartFormat.Line | ChartFormat.Dots),
            new("Линия", ChartFormat.Line),
            new("Точки", ChartFormat.Dots),
        };
    }
}

public class TimeFormatListBoxItem
{
    public string Label;
    public TimeFormat Format;

    public TimeFormatListBoxItem(string label, TimeFormat format)
    {
        Label = label;
        Format = format;
    }

    public override string ToString()
    {
        return Label;
    }
}

public class ChartFormatListBoxItem
{
    public string Label;
    public ChartFormat Format;

    public ChartFormatListBoxItem(string label, ChartFormat format)
    {
        Label = label;
        Format = format;
    }

    public override string ToString()
    {
        return Label;
    }
}
