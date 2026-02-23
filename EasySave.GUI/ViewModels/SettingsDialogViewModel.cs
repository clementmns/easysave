using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyLog;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Resources;

namespace EasySave.GUI.ViewModels;

public partial class SettingsDialogViewModel : ViewModelBase
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
        _selectedLogMode = SettingsService.GetInstance.Settings.LogMode;
        _logServerHost = SettingsService.GetInstance.Settings.LogServerHost;
        _logServerPort = SettingsService.GetInstance.Settings.LogServerPort;

        // Load existing crypted extensions
        var existingExtensions = SettingsService.GetInstance.Settings.CryptExtensions;
        _cryptedExtensions = new ObservableCollection<string>(existingExtensions);
        _cryptoSoftPath = SettingsService.GetInstance.Settings.CryptoSoftPath;
    }

    public List<string> Languages => LanguageMap.Values.ToList();
    [ObservableProperty]
    [Required]
    private string _selectedLanguage;
    
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
    [ObservableProperty]
    [Required]
    private LogFormat _selectedLogFormat;
    
    public List<LogMode> LogModes { get; } = [LogMode.Local, LogMode.Remote, LogMode.Both];
    [ObservableProperty]
    [Required]
    private LogMode _selectedLogMode;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string? _logServerHost;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private int? _logServerPort;

    [ObservableProperty] private string? _cryptoSoftPath;
    
    [ObservableProperty] private string _businessSoftwareProcessName;
    
    // Crypted Extensions Management
    [ObservableProperty] private ObservableCollection<string> _cryptedExtensions;
    [ObservableProperty] private string _newExtension = string.Empty;
    
    public bool CanSaveSettings => SelectedLogMode is LogMode.Local || 
                                   (SelectedLogMode is LogMode.Remote or LogMode.Both &&  !string.IsNullOrWhiteSpace(LogServerHost) && LogServerPort > 0);

    public bool IsRemoteSettingsVisible => SelectedLogMode is LogMode.Both or LogMode.Remote;
    
    partial void OnSelectedLogModeChanged(LogMode value)
    {
        _ = value; // Suppress unused parameter warning
        OnPropertyChanged(nameof(IsRemoteSettingsVisible));
        OnPropertyChanged(nameof(CanSaveSettings));
    }
    
    partial void OnLogServerHostChanged(string? value) => OnPropertyChanged(nameof(CanSaveSettings));
    partial void OnLogServerPortChanged(int? value) => OnPropertyChanged(nameof(CanSaveSettings));
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
    public async Task BrowseSourceCommand()
    {
        var topLevel = TopLevel.GetTopLevel(_dialogWindow);

        IReadOnlyList<IStorageItem> selected = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Messages.SelectSourcePathFile,
            AllowMultiple = false
        })!;

        if (selected.Count < 1) return;

        var path = selected[0].Path.LocalPath;
        CryptoSoftPath = path;
    }
    
    [RelayCommand]
    public void SaveCommand()
    {
        var languageChanged = SelectedLanguage != SettingsService.GetInstance.Settings.Language;

        if (SelectedLogFormat != SettingsService.GetInstance.Settings.LogFormat)
            SettingsService.GetInstance.ChangeLogFormat(SelectedLogFormat);
        
        if (SelectedLogMode != SettingsService.GetInstance.Settings.LogMode)
            SettingsService.GetInstance.SetLogMode(SelectedLogMode);

        if (CryptoSoftPath != null && CryptoSoftPath != SettingsService.GetInstance.Settings.CryptoSoftPath)
            SettingsService.GetInstance.SetCryptoSoftPath(CryptoSoftPath);

        if (SelectedLogMode is LogMode.Both or LogMode.Remote)
        {
            if (LogServerHost != SettingsService.GetInstance.Settings.LogServerHost || LogServerPort != SettingsService.GetInstance.Settings.LogServerPort)
                if (LogServerHost != null && LogServerPort != null )
                {
                    SettingsService.GetInstance.SetLogServer(LogServerHost, LogServerPort.Value);
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