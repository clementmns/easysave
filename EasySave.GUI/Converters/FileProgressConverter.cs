using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters;

public class FileProgressConverter : IMultiValueConverter
{
    private static readonly FileSizeConverter FileSizeConverter = new();

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

        var sizeText = $"{FileSizeConverter.Convert(transferredSize, null, null, culture)} / {FileSizeConverter.Convert(totalSize, null, null, culture)}";
        var fileText = $"{currentFiles} / {totalFiles}";

        var currentFileName = values[5] as string;

        if (!string.IsNullOrEmpty(currentFileName)) return $"{currentFileName} | {fileText} | {sizeText}";
        return $"{fileText} | {sizeText}";
    }
}