using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.GUI.Resources;

namespace EasySave.GUI.ViewModels;

public partial class EditJobViewModel : ObservableObject
{
    private readonly BackupJob _originalJob;
    private readonly MainViewModel _mainViewModel;
    private readonly Window _dialogWindow;
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _sourcePath;
    [ObservableProperty] private string _destinationPath;
    [ObservableProperty] private string _selectedType;
    
    public ObservableCollection<string> BackupTypes { get; } = new()
    {
        "Full",
        "Differential"
    };

    public EditJobViewModel(MainViewModel mainViewModel, BackupJob job, Window? dialogWindow = null)
    {
        _originalJob = job;
        _dialogWindow = dialogWindow;
        _mainViewModel = mainViewModel;
        
        Name = job.Name;
        SourcePath = job.SourcePath;
        DestinationPath = job.DestinationPath;
        SelectedType = job.Type.ToString();
    }
    
    [RelayCommand]
    public void Cancel()
    {
        _dialogWindow.Close(); 
    }
    
    [RelayCommand]
    public void Save()
    {
        _originalJob.Name = Name;
        _originalJob.SourcePath = SourcePath;
        _originalJob.DestinationPath = DestinationPath;
        _originalJob.Type = Enum.Parse<BackupType>(SelectedType);

        
        _mainViewModel.UpdateJob(_originalJob);
        _dialogWindow.Close(); 
    }
    
    [RelayCommand]
    public async Task BrowseSource()
    {
        var topLevel = TopLevel.GetTopLevel(_dialogWindow);
    
        var selected = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "",
            AllowMultiple = false
        })!;
    
        if (selected.Count < 1) return;

        var path = selected[0].Path.LocalPath;
        SourcePath = path;
    }

    [RelayCommand]
    public async Task BrowseDestination()
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
}