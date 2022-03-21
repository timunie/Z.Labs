using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZLabs.Views;

public partial class SensorPlotSettings : UserControl
{
    public SensorPlotSettings()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}