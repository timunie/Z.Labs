using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using ZLabs.Helpers;

namespace ZLabs.Controls;

public class Hyperlink : TemplatedControl
{
    [Required]
    public static readonly StyledProperty<string> UrlProperty = AvaloniaProperty.Register<Hyperlink, string>("Url");
    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<Hyperlink, object>("Content");

    public string Url
    {
        get { return (string) GetValue(UrlProperty); }
        set { SetValue(UrlProperty, value); }
    }

    [Content]
    public object Content
    {
        get { return (object) GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var linkButton = e.NameScope.Find<Button>("LinkButton");
        linkButton.Click += (sender, args) =>
        {
            try
            {
                SystemBasedExtensions.OpenBrowser(Url);
            }
            catch (System.Exception other)
            {
                MessageBoxExtensions.Show(other.Message);
            }
        };
    }
}