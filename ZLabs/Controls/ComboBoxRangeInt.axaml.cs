using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ZLabs.Controls;

public partial class ComboBoxRangeInt : UserControl
{
    public int SelectedIndex { get; set; }

    public int Min { get; set; } = 1;
    public int Max { get; set; } = 10;
    public int Step { get; set; } = 1;
    public ObservableCollection<int> Items { get; set; }
    
    public ComboBoxRangeInt()
    {
        
        if (Step == 0)
            Step = 1;
        if (Min > Max)
        {
            (Min, Max) = (Max, Min);
        }
        var count = (Max - Min) / Step;
        var val = Min - Step;
        var items = Enumerable.Range(0, count).Select(_ => val += Step).ToArray();
        Items = new ObservableCollection<int>(items);
        
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}