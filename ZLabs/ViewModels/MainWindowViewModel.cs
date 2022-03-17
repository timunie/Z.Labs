using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Platform;
using DynamicData.Binding;
using ZLabs.Models;

namespace ZLabs.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<SidebarElement> Sensors { get; }

        public MainWindowViewModel()
        {
            var sensors = Enumerable.Range(1, 10)
                .Select(i =>
                    new SidebarElement("testName", "/Assets/Img/Sensors/Pressure.png")
                );
            Sensors = new ObservableCollection<SidebarElement>(sensors);
        }
    }
}