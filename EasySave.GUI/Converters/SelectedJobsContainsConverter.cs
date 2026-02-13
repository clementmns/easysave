using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Data.Converters;

namespace EasySave.GUI.Converters;

public class SelectedJobsContainsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (values.Count < 2) return false;

        var selectedJobs = values[0] as IEnumerable;
        var job = values[1];

        if (selectedJobs == null || job == null) return false;

        foreach (var item in selectedJobs)
        {
            if (Equals(item, job)) return true;
        }

        return false;
    }
}
