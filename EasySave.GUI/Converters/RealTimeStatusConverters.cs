using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using EasySave.Core.Model;
using EasySave.GUI.Resources;

namespace EasySave.GUI.Converters;

internal static class RealTimeStatusPalette
{
    public sealed record TagColors(IBrush Background, IBrush Border, IBrush Foreground, string Text);

    private static readonly Lazy<TagColors> Ready = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#FFF8E1")),
        new SolidColorBrush(Color.Parse("#F3B340")),
        new SolidColorBrush(Color.Parse("#92400E")),
        Messages.StatusReady));

    private static readonly Lazy<TagColors> Done = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#E6F4FF")),
        new SolidColorBrush(Color.Parse("#4B9CF9")),
        new SolidColorBrush(Color.Parse("#1D4ED8")),
        Messages.StatusDone));

    private static readonly Lazy<TagColors> Error = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#FEE2E2")),
        new SolidColorBrush(Color.Parse("#DC2626")),
        new SolidColorBrush(Color.Parse("#B91C1C")),
        Messages.StatusError));

    private static readonly Lazy<TagColors> OnGoing = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#DFF7DF")),
        new SolidColorBrush(Color.Parse("#7BC47B")),
        new SolidColorBrush(Color.Parse("#166534")),
        Messages.StatusOnGoing));

    private static TagColors GetPalette(RealTimeState.RealTimeStatus status) => status switch
    {
        RealTimeState.RealTimeStatus.Done => Done.Value,
        RealTimeState.RealTimeStatus.Error => Error.Value,
        RealTimeState.RealTimeStatus.OnGoing => OnGoing.Value,
        _ => Ready.Value
    };

    public static TagColors From(object? value)
    {
        var status = value as RealTimeState.RealTimeStatus? ?? RealTimeState.RealTimeStatus.Ready;
        return GetPalette(status);
    }
}

public class RealTimeStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return RealTimeStatusPalette.From(value).Background;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

public class RealTimeStatusToBorderBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return RealTimeStatusPalette.From(value).Border;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

public class RealTimeStatusToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return RealTimeStatusPalette.From(value).Foreground;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

public class RealTimeStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return RealTimeStatusPalette.From(value).Text;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}