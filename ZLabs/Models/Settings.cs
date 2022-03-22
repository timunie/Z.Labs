using System;
using ZLabs.ViewModels;

namespace ZLabs.Models;

public static class Settings
{
    public static double ExperimentTime { get; set; } = 1;
    public static TimeUnit TimeUnit { get; set; } = TimeUnit.TimeUnits[0];
    
    public static TimeFormat TimeFormat { get; set; } = TimeFormat.Stopwatch;
    public static ChartFormat ChartFormat { get; set; } = ChartFormat.Dots | ChartFormat.Line;
}

[Flags]
public enum ChartFormat
{
    Line,
    Dots
}

public enum TimeFormat
{
    Stopwatch,
    MinutesSeconds,
    HoursMinutes
}

public class TimeUnit
{
    public string Label { get; }
    public double Multiplier{ get; }
    
    public static TimeUnit[]  TimeUnits = new TimeUnit[]
    {
        new("Секунда", 1),
        new("Минута", 1 / 60d),
        new("Час", 1 / 60d / 60d),
    };
    
    public TimeUnit(string label, double multiplier)
    {
        Label = label;
        Multiplier = multiplier;
    }
    
    public override string ToString()
    {
        return Label;
    }
}