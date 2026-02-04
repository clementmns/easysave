using System.Collections.ObjectModel;
using System.Text.Json;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Utils;

namespace EasySave.ConsoleApp.Service;

public class BackupJobService
{
    public ObservableCollection<BackupJob>? Jobs { get; set; }
    
    private string _stateFilePath { get; set; }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };
    
    public BackupJobService(string appDirectory)
    {
        if (!FileUtils.DirectoryExists(appDirectory))
        {
            FileUtils.CreateDirectory(appDirectory);
        }

        _stateFilePath = Path.Combine(appDirectory, "state.json");
        Jobs = LoadJobs();
        SubscribeToJobStates();
    }
    
    public void ExecuteJob(BackupJob job)
    {
        EnsureStateSubscription(job);
        var executor = new BackupExecutor();
        executor.ExecuteJob(job);
        UpdateJob(job);
    }

    public bool CreateJob(BackupJob job)
    {
        try
        {
            Jobs?.Add(job);
            EnsureStateSubscription(job);
            if (Jobs != null) SaveJobs(Jobs);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool DeleteJob(BackupJob job)
    {
        try
        {
            RemoveStateSubscription(job);
            Jobs?.Remove(job);
            if (Jobs != null) SaveJobs(Jobs);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool UpdateJob(BackupJob job)
    {
        try
        {
            var oldJob = Jobs?.FirstOrDefault(j => j.Id == job.Id);
            if (oldJob != null)
            {
                RemoveStateSubscription(oldJob);
                Jobs?.Remove(oldJob);
            }

            Jobs?.Add(job);
            EnsureStateSubscription(job);
            if (Jobs != null) SaveJobs(Jobs);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public ObservableCollection<BackupJob>? LoadJobs()
    {
        try
        {
            if (!FileUtils.FileExists(_stateFilePath)) SaveJobs([]);
            var json = File.ReadAllText(_stateFilePath);
            return JsonSerializer.Deserialize<ObservableCollection<BackupJob>>(json, JsonOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private bool SaveJobs(ObservableCollection<BackupJob> jobs)
    {
        try
        {
            var json = JsonSerializer.Serialize(jobs, JsonOptions);
            File.WriteAllText(_stateFilePath, json);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SubscribeToJobStates()
    {
        if (Jobs == null)
        {
            return;
        }

        foreach (var job in Jobs)
        {
            EnsureStateSubscription(job);
        }
    }

    private void EnsureStateSubscription(BackupJob job)
    {
        job.State.PropertyChanged -= OnStateChanged;
        job.State.PropertyChanged += OnStateChanged;
    }

    private void RemoveStateSubscription(BackupJob job)
    {
        job.State.PropertyChanged -= OnStateChanged;
    }

    private void OnStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (Jobs != null)
        {
            SaveJobs(Jobs);
        }
    }
}
