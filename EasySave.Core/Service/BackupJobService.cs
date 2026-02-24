using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using EasyLog;
using EasySave.Core.Model;
using EasySave.Core.Utils;

namespace EasySave.Core.Service;

public class BackupJobService : IRealTimeStateObserver
{
    public ObservableCollection<BackupJob>? Jobs { get; }
    
    private string _stateFilePath { get; }
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly SynchronizationContext? _uiContext;
    private readonly Timer _businessSoftwareTimer;
    private readonly List<BackupJob> _pausedByBusinessSoftware = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };
    
    public BackupJobService()
    {
        _uiContext = SynchronizationContext.Current;
        
        TransferLimitService.Instance.Initialize();
        
        if (Jobs != null) SaveJobs(Jobs);

        var appSaveDirectory = SettingsService.GetInstance.Settings.AppSaveDirectory;
        if (!Directory.Exists(appSaveDirectory)) FileUtils.CreateDirectory(appSaveDirectory);

        _stateFilePath = Path.Combine(appSaveDirectory, "state.json");
        Jobs = LoadJobs();
        SubscribeToJobStates();
        
        _businessSoftwareTimer = new Timer(CheckBusinessSoftware!, null, 0, 2000);
    }

    private void CheckBusinessSoftware(object state)
    {
        var isRunning = ProcessMonitorService.IsBusinessSoftwareRunning;
        
        if (isRunning)
        {
            if (Jobs == null) return;
            foreach (var job in Jobs.Where(j => j.State.Status == RealTimeState.RealTimeStatus.OnGoing))
            {
                PauseJob(job);
                if (!_pausedByBusinessSoftware.Contains(job))
                {
                    _pausedByBusinessSoftware.Add(job);
                }
            }
        }
        else
        {
            if (_pausedByBusinessSoftware.Count > 0)
            {
                var jobsToResume = _pausedByBusinessSoftware.ToList();
                _pausedByBusinessSoftware.Clear();
                
                foreach (var job in jobsToResume)
                {
                    ResumeJob(job);
                }
            }
        }
    }


    public async Task<Dictionary<BackupJob, bool>> ExecuteJobsAsync(IEnumerable<BackupJob> jobs, IProgressionObserver? progressionObserver = null)
    {
        var executor = new BackupExecutor();
        
        var jobsList = jobs.ToList();
        
        var settings = SettingsService.GetInstance.Settings;
        var priorityExtensions = settings.PriorityExtensions;
        
        var priorityJobs = new List<BackupJob>();
        var nonPriorityJobs = new List<BackupJob>();
        
        foreach (var job in jobsList)
        {
            if (FileUtils.HasPriorityFiles(job.SourcePath, priorityExtensions)) priorityJobs.Add(job);
            else nonPriorityJobs.Add(job);
        }
        
        var orderedJobs = priorityJobs.Concat(nonPriorityJobs).ToList();
        
        var tasks = orderedJobs.Select(async job =>
        {
            // Skip jobs that are already running or paused 
            if (job.State.Status is RealTimeState.RealTimeStatus.OnGoing
                                 or RealTimeState.RealTimeStatus.Paused)
                return (job, false);

            Logger.Instance.Write(new LogEntry("Going to execute job", job));

            try
            {
                Stopwatch sw = new();
                sw.Start();

                job.State.AttachStateObserver(this);
                job.State.IsActive = true;
                job.State.Progression = 0;
                job.State.Status = RealTimeState.RealTimeStatus.OnGoing;

                if (progressionObserver != null)
                    job.State.AttachProgressionObserver(progressionObserver);

                var success = await executor.ExecuteJobAsync(job);

                if (!success)
                    throw new Exception("Failed to execute job");

                job.State.Status = RealTimeState.RealTimeStatus.Done;

                sw.Stop();

                Logger.Instance.Write(
                    new LogEntry("Job executed", job, false, sw.ElapsedMilliseconds));

                UpdateJob(job);

                return (job, true);
            }
            catch (OperationCanceledException)
            {
                job.State.Status = RealTimeState.RealTimeStatus.Ready;
                return (job, false);
            }
            catch (Exception e)
            {
                job.State.Status = RealTimeState.RealTimeStatus.Error;

                Logger.Instance.Write(
                    new LogEntry($"Failed to execute job: {e.Message}", job, true));

                return (job, false);
            }
            finally
            {
                job.State.Reset();
                job.State.DetachStateObserver(this);

                if (progressionObserver != null)
                    job.State.DetachProgressionObserver(progressionObserver);
            }
        });

        var results = await Task.WhenAll(tasks);

        return results.ToDictionary(r => r.job, r => r.Item2);
    }

    public bool CreateJob(BackupJob job)
    {
        Logger.Instance.Write(new LogEntry("Going to create job", job));
        try
        {
            job.State.AttachStateObserver(this);
            if (Jobs != null) SaveJobs(Jobs);

            Logger.Instance.Write(new LogEntry("Job created", job));
            PostToUiThread(() => Jobs?.Add(job));
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
            if (Jobs != null) SaveJobs(Jobs);

            Logger.Instance.Write(new LogEntry("Job deleted", job));
            PostToUiThread(() => Jobs?.Remove(job));
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
            if (Jobs == null) return;

            var existingJob = Jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existingJob == null)
            {
                job.State.AttachStateObserver(this);
                PostToUiThread(() => Jobs?.Add(job));
            }
            else
            {
                existingJob.Name = job.Name;
                existingJob.SourcePath = job.SourcePath;
                existingJob.DestinationPath = job.DestinationPath;
                existingJob.Type = job.Type;
                existingJob.State = job.State;
            }
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

        foreach (var job in sorted)
        {
            // Reset job state
            job.State.Reset();
            job.State.Progression = 0;
            job.State.IsActive = false;
            job.State.CurrentFileSize = 0;
            job.State.RemainingFilesSize = job.State.FileSize;
            job.State.RemainingFiles = job.State.TotalFiles;
            job.State.Status = RealTimeState.RealTimeStatus.Ready;
        }

        jobs = new ObservableCollection<BackupJob>(sorted);
        return jobs;
    }

    private void SaveJobs(ObservableCollection<BackupJob> jobs)
    {
        _lock.EnterWriteLock();
        try
        {
            var orderedJobs = jobs.OrderBy(j => j.Id).ToList();
            var json = JsonSerializer.Serialize(orderedJobs, JsonOptions);
            File.WriteAllText(_stateFilePath, json);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
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
    
    private void PostToUiThread(Action action)
    {
        if (_uiContext != null) _uiContext.Post(_ => action(), null);
        else action();
    }

    public void OnStateUpdated(RealTimeState state)
    {
        if (Jobs != null) SaveJobs(Jobs);
    }
    
    public void PauseJob(BackupJob job)
    {
        if (job.State.Status != RealTimeState.RealTimeStatus.OnGoing) return;
        job.PauseGate.Reset();
        job.State.Status = RealTimeState.RealTimeStatus.Paused;
        Logger.Instance.Write(new LogEntry("Job paused", job));
    }
    
    public void ResumeJob(BackupJob job)
    {
        if (job.State.Status != RealTimeState.RealTimeStatus.Paused) return;
        
        // Prevent manual resume if the job was paused by business software
        if (_pausedByBusinessSoftware.Contains(job))
        {
            Logger.Instance.Write(new LogEntry("Cannot resume job: blocked by business software", job, true));
            return;
        }

        job.State.Status = RealTimeState.RealTimeStatus.OnGoing;
        job.PauseGate.Set();
        Logger.Instance.Write(new LogEntry("Job resumed", job));
    }
    
    public void StopJob(BackupJob job)
    {
        if (job.State.Status is not (RealTimeState.RealTimeStatus.OnGoing or RealTimeState.RealTimeStatus.Paused))
            return;
        
        job.PauseGate.Set();
        job.CancellationTokenSource.Cancel();
        Logger.Instance.Write(new LogEntry("Job stopped", job));
    }
}
