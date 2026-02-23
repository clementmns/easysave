using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.GUI.Resources;
using EasySave.GUI.Views;

namespace EasySave.GUI.ViewModels;

public partial class MainViewModel : ViewModelBase, IProgressionObserver, IDisposable
{
    /// <summary>
    /// Singleton instance of the BackupJobService.
    /// </summary>
    private BackupJobService _jobService { get; set; }
    
    /// <summary>
    /// Timer for refreshing business software status
    /// </summary>
    private readonly Timer _businessSoftwareTimer;
    
    /// <summary>
    /// List of current backup jobs.
    /// </summary>
    public ObservableCollection<BackupJob>? Jobs => _jobService.Jobs;
    
    [ObservableProperty] private ObservableCollection<BackupJob> _selectedJobs = [];
    
    public MainViewModel()
    {
        _jobService = new BackupJobService();
        
        // Initialize timer for business software monitoring
        _businessSoftwareTimer = new Timer(2000); // Check every 2 seconds
        _businessSoftwareTimer.Elapsed += (sender, e) => Dispatcher.UIThread.Post(RefreshBusinessSoftwareStatus);
        _businessSoftwareTimer.AutoReset = true;
        _businessSoftwareTimer.Start();
        
        LanguageManager.LanguageChanged += OnLanguageChanged;
        OnPropertyChanged(nameof(Jobs));
        
        RefreshBusinessSoftwareStatus();
    }

    private void OnLanguageChanged()
    {
        if (Jobs is not { } jobs) return;
        foreach (var job in jobs) job.State.RefreshDisplay();
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
    public async Task ExecuteSelectedJobs()
    {
        if (SelectedJobs.Count == 0)
            return;

        await _jobService.ExecuteJobsAsync(SelectedJobs.ToList(), this);
    }
    
    [RelayCommand]
    public async Task ExecuteJobCommand(BackupJob job) => await ExecuteJob(job, this);

    [RelayCommand]
    public async Task ExecuteAllJobs()
    {
        if (Jobs == null || Jobs.Count == 0)
            return;

        await _jobService.ExecuteJobsAsync(Jobs.ToList(), this);
    }

    [RelayCommand]
    public void DeleteSelectedJobs()
    {
        if (SelectedJobs.Count == 0) return;
        foreach (var job in SelectedJobs.ToList()) DeleteJob(job);
        SelectedJobs.Clear();
        OnPropertyChanged(nameof(AreAllJobsSelected));
        OnPropertyChanged(nameof(SelectedJobs));
    }

    [RelayCommand]
    public void OpenCreateJobDialog(Window mainWindow)
    {
        var dialog = new Window
        {
            Title = Messages.createJobTitle,
            Content = new DialogCreateJob(),
            Width = 1000,
            Height = 470,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        dialog.DataContext = new CreateJobDialogViewModel(this, dialog);
        dialog.ShowDialog(mainWindow);
    }

    [RelayCommand]
    public void OpenEditJobDialog(BackupJob jobToEdit)
    {
        var dialog = new Window 
        {
            Title = Messages.editJob,
            Content = new DialogEditJob(),
            Width = 1000,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        
        dialog.DataContext = new EditJobViewModel(this, jobToEdit, dialog);;
        dialog.Show();
    }
    
    [RelayCommand]
    public void OpenSettingsDialog(Window mainWindow)
    {
        var dialog = new Window
        {
            Title = Messages.settings, 
            Content = new DialogSettings(),
            Width = 500,
            Height = 430,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        dialog.DataContext = new SettingsDialogViewModel(dialog);
        dialog.ShowDialog(mainWindow);
    }

    public void AddJob(BackupJob job) => _jobService.CreateJob(job);
    
    public bool IsBusinessSoftwareRunning => ProcessMonitorService.IsBusinessSoftwareRunning;
    
    /// <summary>
    /// Refresh the business software running status for UI updates
    /// </summary>
    public void RefreshBusinessSoftwareStatus()
    {
        try
        {
            OnPropertyChanged(nameof(IsBusinessSoftwareRunning));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing business software status: {ex.Message}");
        }
    }
    
    private void DeleteJob(BackupJob job) => _jobService.DeleteJob(job);
    
    public async Task<bool> ExecuteJob(BackupJob job, IProgressionObserver? progressionObserver = null)
    {
        var result = await _jobService.ExecuteJobsAsync(new[] { job }, progressionObserver);
        return result.TryGetValue(job, out var success) && success;
    }

    public void UpdateJob(BackupJob job)
    {
        _jobService.UpdateJob(job);
        // Job already implements INotifyPropertyChanged = no need to refresh entire collections
    }

    public void OnProgressionUpdated(int progression)
    {
        // no need to refresh the entire collections too
        // BackupJob and RealTimeState already implement INotifyPropertyChanged
        // the UI will automatically update when individual job properties change
    }

    /// <summary>
    /// Execute jobs from command line arguments.
    /// </summary>
    /// <param name="args">1-3 for 1 to 3 or 1,3 for 1 and 3</param>
    /// <returns></returns>
    public async Task<Dictionary<int, bool>> ExecuteJobsFromArgs(string? args)
    {
        var resultMap = new Dictionary<int, bool>();

        if (string.IsNullOrWhiteSpace(args) || Jobs == null)
            return resultMap;

        var requestedIndices = new List<int>();
        var parts = args.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (part.Contains('-'))
            {
                var range = part.Split('-');
                if (range.Length == 2 &&
                    int.TryParse(range[0], out var start) &&
                    int.TryParse(range[1], out var end))
                {
                    for (var i = start; i <= end; i++)
                        if (i > 0) requestedIndices.Add(i);
                }
            }
            else if (int.TryParse(part, out var value) && value > 0)
            {
                requestedIndices.Add(value);
            }
        }

        var jobsToExecute = requestedIndices
            .Select(idx => (idx, jobIdx: idx - 1))
            .Where(x => x.jobIdx >= 0 && x.jobIdx < Jobs.Count)
            .ToList();

        var jobs = jobsToExecute.Select(x => Jobs[x.jobIdx]).ToList();

        var executionResults = await _jobService.ExecuteJobsAsync(jobs, this);

        foreach (var (idx, jobIdx) in jobsToExecute)
        {
            var job = Jobs[jobIdx];
            resultMap[idx] = executionResults.TryGetValue(job, out var success) && success;
        }

        return resultMap;
    }
    
    public void Dispose()
    {
        _businessSoftwareTimer.Stop();
        _businessSoftwareTimer?.Dispose();
    }
}