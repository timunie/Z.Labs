using System;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ZLabs.Helpers;

/// <summary>
/// <para>
/// Converts a string path to a bitmap asset.
/// </para>
/// <para>
/// The asset must be in the same assembly as the program. If it isn't,
/// specify "avares://<assemblynamehere>/" in front of the path to the asset.
/// </para>
/// </summary>
public class BitmapAssetValueConverter : IValueConverter
{
    private static BitmapAssetValueConverter? _instance;
    public static BitmapAssetValueConverter Instance => _instance ?? (_instance = new BitmapAssetValueConverter());

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        if (value is not string rawUri || !targetType.IsAssignableFrom(typeof(Bitmap)))
            throw new NotSupportedException();


        return Convert(rawUri);
    }

    public Bitmap? Convert(string? rawUri)
    {
        if (rawUri == null)
            return null;
        
        Uri uri;

        // Allow for assembly overrides
        if (rawUri.StartsWith("avares://"))
        {
            uri = new Uri(rawUri);
        }
        else
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            uri = new Uri($"avares://{assemblyName}{rawUri}");
        }

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets.Open(uri);

        return new Bitmap(asset);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

