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
    
    [ObservableProperty] public string _name;
    [ObservableProperty] public string _sourcePath;
    [ObservableProperty] public string _destinationPath;
    [ObservableProperty] public string _type;

    public EditJobViewModel(MainViewModel mainViewModel, BackupJob job, Window dialogWindow)
    {
        _originalJob = job;
        _dialogWindow = dialogWindow;
        _mainViewModel = mainViewModel;
        
        Name = job.Name;
        SourcePath = job.SourcePath;
        DestinationPath = job.DestinationPath;
        Type = job.Type.ToString();
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
        
        _mainViewModel.UpdateJob(_originalJob);
        _dialogWindow.Close(); 
    }
}