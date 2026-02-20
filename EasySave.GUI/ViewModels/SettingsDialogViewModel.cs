using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyLog;
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
    
    public List<LogMode> LogModes { get; } = [LogMode.Local, LogMode.Remote, LogMode.Both];
    [ObservableProperty] private LogMode _selectedLogMode;
    
    [ObservableProperty] private string? _logServerHost;
    [ObservableProperty] private int _logServerPort;
    
    [ObservableProperty] private string _businessSoftwareProcessName;
    
    // Crypted Extensions Management
    [ObservableProperty] private ObservableCollection<string> _cryptedExtensions;
    [ObservableProperty] private string _newExtension = string.Empty;
    
    public bool CanSaveSettings => SelectedLogMode is LogMode.Local || 
                                   (SelectedLogMode is LogMode.Remote or LogMode.Both &&  !string.IsNullOrWhiteSpace(LogServerHost) && LogServerPort > 0);

    public bool IsRemoteSettingsVisible => SelectedLogMode is LogMode.Both or LogMode.Remote;
    
    partial void OnSelectedLogModeChanged(LogMode value)
    {
        OnPropertyChanged(nameof(IsRemoteSettingsVisible));
        OnPropertyChanged(nameof(CanSaveSettings));
    }
    
    partial void OnLogServerHostChanged(string? value) => OnPropertyChanged(nameof(CanSaveSettings));
    partial void OnLogServerPortChanged(int value) => OnPropertyChanged(nameof(CanSaveSettings));
    partial void OnSelectedLanguageChanged(string value) => OnPropertyChanged(nameof(SelectedLanguageDisplay));

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
    public void SaveCommand()
    {
        var languageChanged = SelectedLanguage != SettingsService.GetInstance.Settings.Language;

        if (SelectedLogFormat != SettingsService.GetInstance.Settings.LogFormat)
            SettingsService.GetInstance.ChangeLogFormat(SelectedLogFormat);
        
        if (SelectedLogMode != SettingsService.GetInstance.Settings.LogMode)
            SettingsService.GetInstance.SetLogMode(SelectedLogMode);

        if (SelectedLogMode is LogMode.Both or LogMode.Remote)
        {
            if (LogServerHost != SettingsService.GetInstance.Settings.LogServerHost || LogServerPort != SettingsService.GetInstance.Settings.LogServerPort)
                if (LogServerHost != null)
                {
                    SettingsService.GetInstance.SetLogServer(LogServerHost, LogServerPort);
                }
        }
        
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