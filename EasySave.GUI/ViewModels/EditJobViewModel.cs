using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;

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

    public EditJobViewModel(MainViewModel mainViewModel, BackupJob job, Window dialogWindow)
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
}