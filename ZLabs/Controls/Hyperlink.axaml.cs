using System;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ZLabs.Controls;

public partial class Hyperlink : UserControl
{
    public static readonly AvaloniaProperty<string> UrlProperty = AvaloniaProperty.Register<Hyperlink, string>(nameof(Url));

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }
    

    
    public Hyperlink()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(Url);
    }
}