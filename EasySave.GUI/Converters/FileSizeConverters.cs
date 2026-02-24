using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters;

public class FileSizeConverter : IValueConverter
{
    public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes) return "0 B";
        if (bytes == 0) return "0 B";

        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        var order = 0;
        double len = bytes;
            
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
            
        return $"{len:0.##} {sizes[order]}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}