using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Resources;

namespace EasySave.GUI.ViewModels;

public partial class CreateJobDialogViewModel : DialogViewModel
{
    private readonly MainViewModel _mainViewModel;
    private readonly Window? _dialogWindow;
    
    public CreateJobDialogViewModel(MainViewModel mainViewModel, Window? dialogWindow = null)
    {
        _mainViewModel = mainViewModel;
        _dialogWindow = dialogWindow;
    }
    
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _sourcePath;
    [ObservableProperty] private string? _destinationPath;
    [ObservableProperty] private bool _isFileSelected = true;
    [ObservableProperty] private bool _isFolderSelected;
    public List<BackupType> BackupTypes { get; } = [BackupType.Full, BackupType.Differential];
    [ObservableProperty] private BackupType _selectedBackupType = BackupType.Full;
    
    [ObservableProperty] private string? _errorMessage;
    partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(IsErrorVisible));
    
    public bool IsErrorVisible => !string.IsNullOrWhiteSpace(ErrorMessage);
    
    public bool CanCreate => !string.IsNullOrWhiteSpace(Name) && 
                            !string.IsNullOrWhiteSpace(SourcePath) && 
                            !string.IsNullOrWhiteSpace(DestinationPath);

    partial void OnNameChanged(string? value) => OnPropertyChanged(nameof(CanCreate));
    partial void OnSourcePathChanged(string? value) => OnPropertyChanged(nameof(CanCreate));
    partial void OnDestinationPathChanged(string? value) => OnPropertyChanged(nameof(CanCreate));

    [RelayCommand]
    public async Task BrowseSourceCommand()
    {
        var topLevel = TopLevel.GetTopLevel(_dialogWindow);
        IReadOnlyList<IStorageItem> selected;
        
        if (IsFileSelected)
        {
            selected = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Messages.SelectSourcePathFile,
                AllowMultiple = false
            })!;
        }
        else
        {
            selected = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = Messages.SelectSourcePathFolder,
                AllowMultiple = false
            })!;
        }
        
        if (selected.Count < 1) return;

        var path = selected[0].Path.LocalPath;
        SourcePath = path;
    }

    [RelayCommand]
    public async Task BrowseDestinationCommand()
    {
        var topLevel = TopLevel.GetTopLevel(_dialogWindow);
    
        var selected = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Messages.SelectDestinationPath,
            AllowMultiple = false
        })!;
        if (selected.Count < 1) return;

        var path = selected[0].Path.LocalPath;
        DestinationPath = path;
    }

    [RelayCommand]
    public void CancelCommand() => _dialogWindow?.Close();

    [RelayCommand]
    public void CreateCommand()
    {
        try
        {
            if (!CanCreate) return;
            ErrorMessage = null;
            
            var factory = BackupJobFactory.GetInstance();
            var newJob = factory.CreateJob(
                Name, 
                SourcePath, 
                DestinationPath, 
                SelectedBackupType, 
                _mainViewModel.Jobs?.ToList()
            );
            
            _mainViewModel.AddJob(newJob);
            _dialogWindow?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _dialogWindow?.Focus();
        }
    }
}