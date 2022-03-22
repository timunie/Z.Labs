using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;

namespace ZLabs.Helpers;

public class MessageBoxExtensions
{
    public static IMsBoxWindow<ButtonResult> Show(string message, string title = "Info")
    {
        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow(title, message);
        messageBoxStandardWindow.Show();
        return messageBoxStandardWindow;
    }
}