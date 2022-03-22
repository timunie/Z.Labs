using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZLabs.Views;

public partial class SensorsSet : UserControl
{
    public SensorsSet()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}