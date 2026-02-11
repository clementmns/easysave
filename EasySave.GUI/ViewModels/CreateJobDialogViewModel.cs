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

namespace EasySave.GUI.ViewModels;

public partial class CreateJobDialogViewModel : DialogViewModel
{
    private readonly BackupViewModel _backupViewModel;
    private readonly Window? _dialogWindow;
    
    public CreateJobDialogViewModel(BackupViewModel backupViewModel, Window? dialogWindow = null)
    {
        _backupViewModel = backupViewModel;
        _dialogWindow = dialogWindow;
    }
    
    [ObservableProperty] private string? _name;
    
    [ObservableProperty] private string? _sourcePath;
    
    [ObservableProperty] private string? _destinationPath;

    [ObservableProperty] private bool _isFileSelected = true;
    
    [ObservableProperty] private bool _isFolderSelected = true;
    
    public List<BackupType> BackupTypes { get; } = [BackupType.Full, BackupType.Differential];

    [ObservableProperty]
    private BackupType _selectedBackupType = BackupType.Full;

    [RelayCommand]
    public async Task BrowseSourceCommand()
    {
        var topLevel = TopLevel.GetTopLevel(_dialogWindow);
        IReadOnlyList<IStorageItem> selected;
        
        if (_isFileSelected)
        {
            selected = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file",
                AllowMultiple = false
            })!;
        }
        else
        {
            selected = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select a folder",
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
            Title = "Select a folder to save backups in",
            AllowMultiple = false
        })!;
        if (selected.Count < 1) return;

        var path = selected[0].Path.LocalPath;
        DestinationPath = path;
    }

    [RelayCommand]
    public void CancelCommand()
    {
        _dialogWindow?.Close();
    }

    [RelayCommand]
    public void CreateCommand()
    {
        try
        {
            // validate inputs
            if (string.IsNullOrWhiteSpace(Name) || 
                string.IsNullOrWhiteSpace(SourcePath) || 
                string.IsNullOrWhiteSpace(DestinationPath))
            {
                // TODO: Show error message to user
                return;
            }
            
            // create the job using the factory
            var factory = BackupJobFactory.GetInstance();
            var newJob = factory.CreateJob(
                Name, 
                SourcePath, 
                DestinationPath, 
                SelectedBackupType, 
                _backupViewModel.Jobs?.ToList()
            );
            
            // Add the job to the backup view model
            _backupViewModel.AddJob(newJob);
            
            // Close the dialog after the successful creation
            _dialogWindow?.Close();
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            Console.WriteLine($"Error creating job: {ex.Message}");
        }
    }
}