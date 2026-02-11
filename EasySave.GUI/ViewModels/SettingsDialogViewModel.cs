using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;

namespace EasySave.GUI.ViewModels;

public partial class SettingsDialogViewModel : DialogViewModel
{
    private readonly Window? _dialogWindow;
    
    private static readonly Dictionary<string, string> LanguageMap = new()
    {
        { "fr-FR", "French" },
        { "en-US", "English" },
        { "de-DE", "German" },
        { "es-ES", "Spanish" }
    };
    
    public SettingsDialogViewModel(Window? dialogWindow = null)
    {
        _dialogWindow = dialogWindow;

        _selectedLanguage = SettingsService.GetInstance.Settings.Language;
        _selectedLogFormat = SettingsService.GetInstance.Settings.LogFormat;
    }

    public List<string> Languages => LanguageMap.Values.ToList();
    [ObservableProperty] private string _selectedLanguage;
    
    partial void OnSelectedLanguageChanged(string value) => OnPropertyChanged(nameof(SelectedLanguageDisplay));

    public string SelectedLanguageDisplay
    {
        get => LanguageMap[SelectedLanguage];
        set
        {
            SelectedLanguage = LanguageMap.FirstOrDefault(x => x.Value == value).Key;
            OnPropertyChanged(nameof(SelectedLanguage));
        }
    }

    public List<LogFormat> LogFormats { get; } = [LogFormat.Json, LogFormat.Xml];
    [ObservableProperty] private LogFormat _selectedLogFormat;
    
    [RelayCommand]
    public void CancelCommand() => _dialogWindow?.Close();
    
    [RelayCommand]
    public void SaveCommand()
    {
        if (SelectedLogFormat != SettingsService.GetInstance.Settings.LogFormat) 
            SettingsService.GetInstance.ChangeLogFormat(SelectedLogFormat);
        
        if (SelectedLanguage != SettingsService.GetInstance.Settings.Language)
            SettingsService.GetInstance.SetLanguage(SelectedLanguage);
        
        _dialogWindow?.Close();
    }
}