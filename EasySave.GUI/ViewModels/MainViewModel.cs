using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Views;
using System.Linq;
using EasyLog;

namespace EasySave.GUI.ViewModels;

/// <summary>
/// ViewModel for managing backup jobs, providing methods to add, delete, update, and execute backup jobs.
/// </summary>
public partial class MainViewModel : ViewModelBase, IDisposable
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
    /// Timer for refreshing business software status
    /// </summary>
    private Timer _businessSoftwareTimer;
    
    /// <summary>
    /// List of current backup jobs.
    /// </summary>
    public ObservableCollection<BackupJob>? Jobs => _jobService.Jobs;
    
    public ObservableCollection<BackupJob> SelectedJobs { get; } = new();

    [RelayCommand]
    public void ToggleSelection(BackupJob job)
    {
        if (SelectedJobs.Contains(job))
        {
            SelectedJobs.Remove(job);
        }
        else
        {
            SelectedJobs.Add(job);
        }
    }

    
    public MainViewModel()
    {
        _jobService = new BackupJobService();
        _backupExecutor = new BackupExecutor();
        
        // Initialize timer for business software monitoring
        _businessSoftwareTimer = new Timer(2000); // Check every 2 seconds
        _businessSoftwareTimer.Elapsed += (sender, e) => RefreshBusinessSoftwareStatus();
        _businessSoftwareTimer.AutoReset = true;
        _businessSoftwareTimer.Start();
        
        // Initial refresh
        RefreshBusinessSoftwareStatus();
    }

    [RelayCommand]
    public async Task OpenCreateJobDialog(Window mainWindow)
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
        
        await dialog.ShowDialog(mainWindow);
    }
    
    
    [RelayCommand]
    public async Task ExecuteSelectedJobs()
    {
        if (SelectedJobs.Count == 0) return;
        
        // Refresh the business software status for UI
        RefreshBusinessSoftwareStatus();
    
        foreach (var job in SelectedJobs.ToList())
        {
            await Task.Run(() => ExecuteJob(job));
            
        }
    }
    
    [RelayCommand]
    public async Task OpenSettingsDialog(Window mainWindow)
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

        await dialog.ShowDialog(mainWindow);
    }

    public bool AddJob(BackupJob job) => _jobService.CreateJob(job);
    
    public ProcessMonitorService ProcessMonitor { get; } = ProcessMonitorService.Instance;
    
    public bool IsBusinessSoftwareRunning => ProcessMonitor.IsBusinessSoftwareRunning;
    
    /// <summary>
    /// Refresh the business software running status for UI updates
    /// </summary>
    public void RefreshBusinessSoftwareStatus()
    {
        try
        {
            // Use Avalonia's dispatcher to ensure UI updates happen on the correct thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                OnPropertyChanged(nameof(IsBusinessSoftwareRunning));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing business software status: {ex.Message}");
        }
    }
    
    public bool DeleteJob(BackupJob job) => _jobService.DeleteJob(job); 
    
    public bool ExecuteJob(BackupJob job, IProgressionObserver? progressionObserver = null)
    {
        try
        {
            if (ProcessMonitor.IsBusinessSoftwareRunning)
            {
                Console.WriteLine($"Backup {job.Name} blocked: business software detected");
                Logger.Instance.Write($"Backup blocked for job {job.Name}: business software detected");
                return false;
            }

            Console.WriteLine($"Start : {job.Name}");
        
            job.State.AttachStateObserver(_jobService);
            if (progressionObserver != null) job.State.AttachProgressionObserver(progressionObserver);

            var result = _jobService.ExecuteJob(job);

            job.State.DetachStateObserver(_jobService);
            if (progressionObserver != null) job.State.DetachProgressionObserver(progressionObserver);

            Console.WriteLine($"Job {job.Name} finished with success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Debug : {job.Name}: {ex.Message}");
            return false;
        }
    }

[RelayCommand]
    public void DeleteSelectedJobs()
    {
        if (SelectedJobs.Count == 0) return;
    
        foreach (var job in SelectedJobs.ToList())
        {
            DeleteJob(job);
        }
        SelectedJobs.Clear();
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
    
    public void Dispose()
    {
        _businessSoftwareTimer?.Stop();
        _businessSoftwareTimer?.Dispose();
    }
}