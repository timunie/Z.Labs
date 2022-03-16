using System.Numerics;

namespace ZLabs.Models;

public class SidebarElement
{
    public string Name { get; set; }
    public string ImagePath { get; set; }
    


    public SidebarElement(string name, string imagePath)
    {
        Name = name;
        ImagePath = imagePath;
    }

    public SidebarElement()
    {
    }
}