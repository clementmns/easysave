using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue) return intValue.ToString();
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && int.TryParse(stringValue, out var intValue)) return intValue;
            return 0; // Default value if conversion fails
        }
    }
}
