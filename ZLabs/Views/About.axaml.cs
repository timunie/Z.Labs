using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZLabs.Models;

namespace ZLabs.Views;

public partial class About : UserControl
{
    
    
    public About()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}