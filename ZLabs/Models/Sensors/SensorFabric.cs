using System;
using HardwareLib.Classes;
using ZLabs.Models.Implementations;

namespace ZLabs.Models;

public class SensorFabric
{
    public static Sensor GetSensor(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.TempSensor =>
                new Sensor(
                        "Датчик T эксп.",
                        "/Assets/Img/Sensors/Temp_1.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-40, 165, "°С"),
                        new(-40, 329, "°F", converter: (c) => c * 9 / 5 + 32)
                    }),
            SensorType.AbsolutePressureSensor =>
                new Sensor(
                        "Датчик P абс.",
                        "/Assets/Img/Sensors/Pressure.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(0, 500, "кПа")
                    }),
            SensorType.TeslametrSensor =>
                new Sensor(
                        "Тесламетр",
                        "/Assets/Img/Sensors/Tesla.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-100, 100, "мЛт"),
                        new(-1, 1, "Тл", 1 / 1000d)
                    }),
            SensorType.VoltmeterSensor =>
                new Sensor(
                        "Вольтметр",
                        "/Assets/Img/Sensors/Voltage.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-15, 15, "В")
                    }),
            SensorType.AmpermetrSensor =>
                new Sensor(
                        "Амперметр",
                        "/Assets/Img/Sensors/Amper.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-1, 1, "A"),
                        new(-10, 10, "A")
                    }),
            SensorType.AccelerometerXSensor =>
                new Sensor(
                        "Датчик ускорения. Ось Х",
                        "/Assets/Img/Sensors/Accelerometr.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-2, 2, "g")
                    }),
            SensorType.AccelerometerYSensor =>
                new Sensor(
                        "Датчик ускорения. Ось Y",
                        "/Assets/Img/Sensors/Accelerometr.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-2, 2, "g")
                    }),
            SensorType.AccelerometerZSensor =>
                new Sensor(
                        "Датчик ускорения. Ось Z",
                        "/Assets/Img/Sensors/Accelerometr.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-2, 2, "g")
                    }),
            SensorType.ElectricalConductivitySensor =>
                new Sensor(
                        "Датчик электропроводимости",
                        "/Assets/Img/Sensors/Electrical_conduct.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-100, 100, "мЛт"),
                        new(-1, 1, "Тл", 1 / 1000d)
                    }),
            SensorType.HumiditySensor =>
                new Sensor(
                        "Датчик влажности",
                        "/Assets/Img/Sensors/Humidity.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(0, 100, "%"),
                    }),
            SensorType.LightSensor =>
                new Sensor(
                        "Датчик освещенности",
                        "/Assets/Img/Sensors/Illumination.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(0, 180000, "лк"),
                    }),
            SensorType.TempOutsideSensor =>
                new Sensor(
                        "Датчик Т окр.",
                        "/Assets/Img/Sensors/Temp_2.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(-40, 60, "°С"),
                        new(-40, 140, "°F", converter: (c) => c * 9 / 5 + 32)
                    }),
            SensorType.ColorimeterSensor =>
                new Sensor(
                        "Колориметр",
                        "/Assets/Img/Sensors/Colorimeter.png")
                    .AddRanges(new PlotRange?[]
                    {
                        new(0, 2, "D"),
                    }),
            SensorType.PhSensor =>
                new ModeSensor(
                        "Датчик pH",
                        "/Assets/Img/Sensors/pH.png",
                        new[] {"вольтметр", "Ph"}
                    )
                    .AddRanges(new PlotRange?[]
                    {
                        new (-15, 15, "В"),
                        new (-0, 14, "Ph"),
                    }),
            
            _ => throw new ArgumentOutOfRangeException(nameof(sensorType), sensorType, null)
        };
    }
}