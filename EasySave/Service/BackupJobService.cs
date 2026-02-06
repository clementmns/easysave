using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using EasyLog;
using EasySave.Model;
using EasySave.Utils;

namespace EasySave.Service;

public class BackupJobService : IRealTimeStateObserver
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
        if (!Directory.Exists(appDirectory)) FileUtils.CreateDirectory(appDirectory);

        _stateFilePath = Path.Combine(appDirectory, "state.json");
        Jobs = LoadJobs();
        SubscribeToJobStates();
    }
    
    public bool ExecuteJob(BackupJob job)
    {
        Logger.Instance.Write(new LogEntry("Going to execute job", job));
        try
        {
            // get execution time
            Stopwatch sw = new();
            sw.Start();
            
            job.State.AttachStateObserver(this);
            var executor = new BackupExecutor();
            executor.ExecuteJob(job);
            sw.Stop();
            
            Logger.Instance.Write(new LogEntry("Job executed", job, false, sw.ElapsedMilliseconds));
            UpdateJob(job);
            return true;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(new LogEntry($"Failed to execute job: {e.Message}", job, true));
            return false;
        }
    }

    public bool CreateJob(BackupJob job)
    {
        Logger.Instance.Write(new LogEntry("Going to create job", job));
        try
        {
            Jobs?.Add(job);
            job.State.AttachStateObserver(this);
            SortJobsById();
            if (Jobs != null) SaveJobs(Jobs);
            Logger.Instance.Write(new LogEntry("Job created", job));
            return true;
        }
        catch (Exception)
        {
            Logger.Instance.Write(new LogEntry("Failed to create job", job, true));
            throw;
        }
    }

    public bool DeleteJob(BackupJob job)
    {
        Logger.Instance.Write(new LogEntry("Going to delete job", job));
        try
        {
            RemoveStateSubscription(job);
            Jobs?.Remove(job);
            SortJobsById();
            if (Jobs != null) SaveJobs(Jobs);
            Logger.Instance.Write(new LogEntry("Job deleted", job));
            return true;
        }
        catch (Exception)
        {
            Logger.Instance.Write(new LogEntry("Failed to delete job", job, true));
            throw;
        }
    }

    public void UpdateJob(BackupJob job)
    {
        Logger.Instance.Write(new LogEntry("Going to update job", job));
        try
        {
            var oldJob = Jobs?.FirstOrDefault(j => j.Id == job.Id);
            if (oldJob != null)
            {
                RemoveStateSubscription(oldJob);
                Jobs?.Remove(oldJob);
            }

            Jobs?.Add(job);
            job.State.AttachStateObserver(this);
            SortJobsById();
            if (Jobs != null) SaveJobs(Jobs);
            Logger.Instance.Write(new LogEntry("Job updated", job));
        }
        catch (Exception)
        {
            Logger.Instance.Write(new LogEntry("Failed to update job", job, true));
            throw;
        }
    }

    private ObservableCollection<BackupJob>? LoadJobs()
    {
        if (!File.Exists(_stateFilePath)) SaveJobs([]);
        var json = File.ReadAllText(_stateFilePath);
        var jobs = JsonSerializer.Deserialize<ObservableCollection<BackupJob>>(json, JsonOptions);
        if (jobs == null) return jobs;
        var sorted = jobs.OrderBy(j => j.Id).ToList();
        jobs = new ObservableCollection<BackupJob>(sorted);
        return jobs;
    }

    private void SaveJobs(ObservableCollection<BackupJob> jobs)
    {
        var orderedJobs = jobs.OrderBy(j => j.Id).ToList();
        var json = JsonSerializer.Serialize(orderedJobs, JsonOptions);
        File.WriteAllText(_stateFilePath, json);
    }

    private void SubscribeToJobStates()
    {
        if (Jobs == null) return;
        foreach (var job in Jobs) job.State.AttachStateObserver(this);
    }

    private void RemoveStateSubscription(BackupJob job)
    {
        job.State.DetachStateObserver(this);
    }

    private void SortJobsById()
    {
        if (Jobs == null) return;
        var sorted = Jobs.OrderBy(j => j.Id).ToList();
        Jobs.Clear();
        foreach (var job in sorted) Jobs.Add(job);
    }

    public void OnStateUpdated(RealTimeState state)
    {
        SortJobsById();
        if (Jobs != null) SaveJobs(Jobs);
    }
}
