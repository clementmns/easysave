using System.Collections.ObjectModel;
using System.Text.Json;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Utils;

namespace EasySave.ConsoleApp.Service;

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
    
    public void ExecuteJob(BackupJob job)
    {
        job.State.Attach(this);
        var executor = new BackupExecutor();
        executor.ExecuteJob(job);
        UpdateJob(job);
    }

    public bool CreateJob(BackupJob job)
    {
        try
        {
            Jobs?.Add(job);
            job.State.Attach(this);
            SortJobsById();
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
            SortJobsById();
            if (Jobs != null) SaveJobs(Jobs);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void UpdateJob(BackupJob job)
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
            job.State.Attach(this);
            SortJobsById();
            if (Jobs != null) SaveJobs(Jobs);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private ObservableCollection<BackupJob>? LoadJobs()
    {
        try
        {
            if (!File.Exists(_stateFilePath)) SaveJobs([]);
            var json = File.ReadAllText(_stateFilePath);
            var jobs = JsonSerializer.Deserialize<ObservableCollection<BackupJob>>(json, JsonOptions);
            if (jobs == null) return jobs;
            var sorted = jobs.OrderBy(j => j.Id).ToList();
            jobs = new ObservableCollection<BackupJob>(sorted);
            return jobs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SaveJobs(ObservableCollection<BackupJob> jobs)
    {
        try
        {
            var orderedJobs = jobs.OrderBy(j => j.Id).ToList();
            var json = JsonSerializer.Serialize(orderedJobs, JsonOptions);
            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SubscribeToJobStates()
    {
        if (Jobs == null) return;
        foreach (var job in Jobs) job.State.Attach(this);
    }

    private void RemoveStateSubscription(BackupJob job)
    {
        job.State.Detach(this);
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
