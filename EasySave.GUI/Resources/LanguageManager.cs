using System;
using System.Collections;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace EasySave.GUI.Resources;

public static class LanguageManager
{
    private static readonly ResourceDictionary LanguageResources = new();
    public static event Action? LanguageChanged;

    public static void Initialize(string language)
    {
        if (Application.Current == null) return;

        if (!Application.Current.Resources.MergedDictionaries.Contains(LanguageResources))
        {
            Application.Current.Resources.MergedDictionaries.Add(LanguageResources);
        }

        SetLanguage(language);
    }

    public static void SetLanguage(string language)
    {
        Messages.Culture = CultureInfo.GetCultureInfo(language);
        UpdateResources();
        LanguageChanged?.Invoke();
    }

    private static void UpdateResources()
    {
        if (Application.Current == null) return;

        LanguageResources.Clear();

        var culture = Messages.Culture ?? CultureInfo.CurrentUICulture;
        var resourceSet = Messages.ResourceManager.GetResourceSet(culture, true, true);

        if (resourceSet == null) return;

        foreach (DictionaryEntry entry in resourceSet)
        {
            var key = entry.Key?.ToString();
            if (string.IsNullOrWhiteSpace(key)) continue;

            LanguageResources[key] = entry.Value?.ToString() ?? string.Empty;
        }
    }
}
