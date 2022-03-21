using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ZLabs.Views;

public partial class SensorsPanel : UserControl
{
    public SensorsPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}