using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using EasySave.Core.Model;

namespace EasySave.GUI.Converters;

internal static class RealTimeStatusPalette
{
    public sealed record TagColors(IBrush Background, IBrush Border, IBrush Foreground, string TextResourceKey);

    private const string ReadyKey = "StatusReady";
    private const string DoneKey = "StatusDone";
    private const string ErrorKey = "StatusError";
    private const string OnGoingKey = "StatusOnGoing";
    private const string PausedKey = "StatusPaused";

    private static readonly Lazy<TagColors> Ready = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#FFF8E1")),
        new SolidColorBrush(Color.Parse("#F3B340")),
        new SolidColorBrush(Color.Parse("#92400E")),
        ReadyKey));

    private static readonly Lazy<TagColors> Done = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#E6F4FF")),
        new SolidColorBrush(Color.Parse("#4B9CF9")),
        new SolidColorBrush(Color.Parse("#1D4ED8")),
        DoneKey));

    private static readonly Lazy<TagColors> Error = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#FEE2E2")),
        new SolidColorBrush(Color.Parse("#DC2626")),
        new SolidColorBrush(Color.Parse("#B91C1C")),
        ErrorKey));

    private static readonly Lazy<TagColors> OnGoing = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#DFF7DF")),
        new SolidColorBrush(Color.Parse("#7BC47B")),
        new SolidColorBrush(Color.Parse("#166534")),
        OnGoingKey));

    private static readonly Lazy<TagColors> Paused = new(() => new TagColors(
        new SolidColorBrush(Color.Parse("#F3F0FF")),
        new SolidColorBrush(Color.Parse("#7C3AED")),
        new SolidColorBrush(Color.Parse("#5B21B6")),
        PausedKey));

    private static TagColors GetPalette(RealTimeState.RealTimeStatus status) => status switch
    {
        RealTimeState.RealTimeStatus.Done => Done.Value,
        RealTimeState.RealTimeStatus.Error => Error.Value,
        RealTimeState.RealTimeStatus.OnGoing => OnGoing.Value,
        RealTimeState.RealTimeStatus.Paused => Paused.Value,
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
        var palette = RealTimeStatusPalette.From(value);
        return Application.Current?.FindResource(palette.TextResourceKey) ?? palette.TextResourceKey;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

public class StatusToPlayPauseIconConverter : IValueConverter
{
    public static readonly StatusToPlayPauseIconConverter Instance = new();
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RealTimeState.RealTimeStatus.OnGoing || value is true
            ? "/Assets/svg/pause.svg"
            : "/Assets/svg/play.svg";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}

public class FileProgressConverter : IMultiValueConverter
{
    private static string FormatSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 5) return null;
        
        if (values[0] is not int totalFiles ||
            values[1] is not long remainingFiles ||
            values[2] is not long totalSize ||
            values[3] is not long remainingSize ||
            values[4] is not int progression)
        {
            return string.Empty;
        }

        var currentFiles = totalFiles - (int)remainingFiles;
        var transferredSize = totalSize - remainingSize;

        var sizeText = $"{FormatSize(transferredSize)} / {FormatSize(totalSize)}";
        var fileText = $"{currentFiles} / {totalFiles}";

        var currentFileName = values[5] as string;

        if (!string.IsNullOrEmpty(currentFileName)) return $"{currentFileName} | {fileText} | {sizeText}";
        return $"{fileText} | {sizeText}";
    }
}