using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Resources;

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
        _businessSoftwareProcessName = SettingsService.GetInstance.Settings.BusinessSoftwareProcessName;
        
        // Load existing crypted extensions
        var existingExtensions = SettingsService.GetInstance.Settings.CryptExtensions;
        _cryptedExtensions = new ObservableCollection<string>(existingExtensions);
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
    
    [ObservableProperty] private string _businessSoftwareProcessName = string.Empty;
    
    // Crypted Extensions Management
    [ObservableProperty] private ObservableCollection<string> _cryptedExtensions = [];
    [ObservableProperty] private string _newExtension = string.Empty;
    
    
    [RelayCommand]
    public void CancelCommand() => _dialogWindow?.Close();
    
    [RelayCommand]
    private void AddExtension()
    {
        if (string.IsNullOrWhiteSpace(NewExtension))
            return;
        
        // Ensure extension starts with a dot
        var extension = NewExtension.Trim();
        if (!extension.StartsWith("."))
            extension = "." + extension;
        
        // Check if extension already exists
        if (CryptedExtensions.Contains(extension))
        {
            NewExtension = string.Empty;
            return;
        }
        
        CryptedExtensions.Add(extension);
        NewExtension = string.Empty;
    }
    
    [RelayCommand]
    private void RemoveExtension(string? extension)
    {
        if (extension != null && CryptedExtensions.Contains(extension))
            CryptedExtensions.Remove(extension);
    }
    
    [RelayCommand]
    public async Task SaveCommand()
    {
        var languageChanged = SelectedLanguage != SettingsService.GetInstance.Settings.Language;

        if (SelectedLogFormat != SettingsService.GetInstance.Settings.LogFormat)
            SettingsService.GetInstance.ChangeLogFormat(SelectedLogFormat);
        
        if (!string.IsNullOrWhiteSpace(BusinessSoftwareProcessName) && 
            BusinessSoftwareProcessName != SettingsService.GetInstance.Settings.BusinessSoftwareProcessName)
            SettingsService.GetInstance.SetBusinessSoftwareProcessName(BusinessSoftwareProcessName);
        
        if (languageChanged)
        {
            SettingsService.GetInstance.SetLanguage(SelectedLanguage);
            LanguageManager.SetLanguage(SelectedLanguage);
        }
        
        // Save crypted extensions
        SettingsService.GetInstance.SetCryptedExtensions(CryptedExtensions.ToList());

        _dialogWindow?.Close();
    }
}
