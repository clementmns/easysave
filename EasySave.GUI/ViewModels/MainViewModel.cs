using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private BackupJobService _jobService { get; set; }
    public ObservableCollection<BackupJob>? Jobs => _jobService.Jobs;
    public ObservableCollection<BackupJob> SelectedJobs { get; } = [];
    
    public MainViewModel()
    {
        _jobService = new BackupJobService();
    }

    public bool? AreAllJobsSelected
    {
        get
        {
            if (Jobs == null || Jobs.Count == 0) return false;
            if (SelectedJobs.Count == Jobs.Count) return true;
            if (SelectedJobs.Count == 0) return false;
            return null;
        }
    }
    
    [RelayCommand]
    public void ToggleAllSelectionCommand()
    {
        if (AreAllJobsSelected == true) SelectedJobs.Clear();
        else
        {
            SelectedJobs.Clear();
            if (Jobs != null)
            {
                foreach (var job in Jobs) SelectedJobs.Add(job);
            }
        }
        OnPropertyChanged(nameof(AreAllJobsSelected));
        OnPropertyChanged(nameof(SelectedJobs));
    }

    [RelayCommand]
    public void ToggleSelection(BackupJob job)
    {
        if (SelectedJobs.Contains(job)) SelectedJobs.Remove(job);
        else SelectedJobs.Add(job);
        OnPropertyChanged(nameof(AreAllJobsSelected));
        OnPropertyChanged(nameof(SelectedJobs));
    }
    
    [RelayCommand]
    public void ExecuteSelectedJobs()
    {
        if (SelectedJobs.Count == 0) return;
        foreach (var job in SelectedJobs.ToList()) ExecuteJob(job);
    }
    
    [RelayCommand]
    public void ExecuteJobCommand(BackupJob job) => ExecuteJob(job);

    [RelayCommand]
    public void ExecuteAllJobs()
    {
        if (Jobs == null) return;
        foreach (var job in Jobs.ToList()) ExecuteJob(job);
    }

    [RelayCommand]
    public void DeleteSelectedJobs()
    {
        if (SelectedJobs.Count == 0) return;
        foreach (var job in SelectedJobs.ToList()) DeleteJob(job);
        SelectedJobs.Clear();
    }
    
    [RelayCommand]
    public void OpenCreateJobDialog(Window mainWindow)
    {
        var dialog = new Window
        {
            Title = "Create New job",
            Content = new DialogCreateJob(),
            Width = 1000,
            Height = 470,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        dialog.DataContext = new CreateJobDialogViewModel(this, dialog);
        dialog.ShowDialog(mainWindow);
    }

    [RelayCommand]
    public void OpenSettingsDialog(Window mainWindow)
    {
        var dialog = new Window
        {
            Title = "App Settings",
            Content = new DialogSettings(),
            Width = 500,
            Height = 430,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        dialog.DataContext = new SettingsDialogViewModel(dialog);
        dialog.ShowDialog(mainWindow);
    }

    public void AddJob(BackupJob job) => _jobService.CreateJob(job);

    public void DeleteJob(BackupJob job) => _jobService.DeleteJob(job);

    public bool ExecuteJob(BackupJob job, IProgressionObserver? progressionObserver = null)
    {
        // attach to needed observers
        job.State.AttachStateObserver(_jobService);
        if (progressionObserver != null) job.State.AttachProgressionObserver(progressionObserver);

        var result = _jobService.ExecuteJob(job);

        // detach from observers to avoid memory leaks
        job.State.DetachStateObserver(_jobService);
        if (progressionObserver != null) job.State.DetachProgressionObserver(progressionObserver);

        return result;
    }

    public void UpdateJob(BackupJob job) => _jobService.UpdateJob(job);

    /// <summary>
    /// Execute jobs from command line arguments.
    /// </summary>
    /// <param name="args">1-3 for 1 to 3 or 1;3 for 1 and 3</param>
    /// <returns></returns>
    public Dictionary<int, bool> ExecuteJobsFromArgs(string? args)
    {
        // use a dictionary to return the result of each job
        var resultMap = new Dictionary<int, bool>();
        try
        {
            if (string.IsNullOrWhiteSpace(args)) return resultMap;

            var requestedIndices = new List<int>();
            var parts = args.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                // check for a list of indices
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out var start) && int.TryParse(range[1], out var end))
                    {
                        for (var i = start; i <= end; i++)
                        {
                            if (i > 0) requestedIndices.Add(i);
                        }
                    }
                }
                else if (int.TryParse(part, out var jobNumber) && jobNumber > 0)
                {
                    requestedIndices.Add(jobNumber);
                }
            }

            foreach (var idx in requestedIndices)
            {
                var jobIdx = idx - 1;
                if (Jobs != null && jobIdx >= 0 && jobIdx < Jobs.Count)
                {
                    // execute job and store result in the map
                    resultMap[idx] = ExecuteJob(Jobs[jobIdx]);
                }
                else
                {
                    resultMap[idx] = false;
                }
            }
            return resultMap;
        }
        catch (Exception)
        {
            return resultMap;
        }
    }
}