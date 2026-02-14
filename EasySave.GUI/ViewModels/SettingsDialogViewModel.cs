using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels;

public partial class SettingsDialogViewModel : DialogViewModel
{
    private readonly Window? _dialogWindow;
    
    private static readonly Dictionary<string, string> LanguageMap = new()
    {
        { "fr-FR", "Français" },
        { "en-US", "English" },
        { "de-DE", "Deutsch" },
        { "es-ES", "Español" },
        { "it-IT", "Italiano"}
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
    public async Task SaveCommand()
    {
        var languageChanged = SelectedLanguage != SettingsService.GetInstance.Settings.Language;

        if (SelectedLogFormat != SettingsService.GetInstance.Settings.LogFormat)
            SettingsService.GetInstance.ChangeLogFormat(SelectedLogFormat);
        
        if (languageChanged)
            SettingsService.GetInstance.SetLanguage(SelectedLanguage);
        
        if (languageChanged) await ShowRestartDialogAsync();
        _dialogWindow?.Close();
        
    }

    private async Task ShowRestartDialogAsync()
    {
        var dialog = new Window
        {
            // Title = Messages.RestartTitle,
            Content = new DialogRestartConfirm(),
            Width = 300,
            Height = 140,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        dialog.DataContext = new RestartConfirmDialogViewModel(dialog);

        if (_dialogWindow != null) await dialog.ShowDialog(_dialogWindow);
        else await dialog.ShowDialog(null);
    }
}
