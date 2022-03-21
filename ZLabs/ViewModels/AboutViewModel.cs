using ZLabs.Models;

namespace ZLabs.ViewModels;

public class AboutViewModel : ViewModelBase, IPage
{
    public string ImagePath { get; } = "/Assets/Img/Info.png";
    public string Name { get; } = "О программе";
}