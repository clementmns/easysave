using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Ressources;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels;

/// <summary>
/// ViewModel for managing backup jobs, providing methods to add, delete, update, and execute backup jobs.
/// </summary>
public partial class BackupViewModel : ViewModelBase
{
    /// <summary>
    /// Singleton instance of the BackupJobService.
    /// </summary>
    private BackupJobService _jobService { get; set; }
    
    /// <summary>
    /// Singleton instance of the BackupExecutor.
    /// </summary>
    private BackupExecutor _backupExecutor { get; set; }
    
    /// <summary>
    /// List of current backup jobs.
    /// </summary>
    public ObservableCollection<BackupJob>? Jobs => _jobService.Jobs;
    
    public BackupViewModel()
    {
        _jobService = new BackupJobService();
        _backupExecutor = new BackupExecutor();
    }

    [RelayCommand]
    public async Task OpenCreateJobDialog(Window mainWindow)
    {
        var dialog = new Window() 
        {
            Title = "Create New job",
            Content = new DialogCreateJob(),
            Width = 500,
            Height = 450,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };
        
        dialog.DataContext = new CreateJobDialogViewModel(this, dialog);
        
        await dialog.ShowDialog(mainWindow);
    }

    public bool AddJob(BackupJob job) => _jobService.CreateJob(job);
    
    public bool DeleteJob(BackupJob job) => _jobService.DeleteJob(job);
    
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