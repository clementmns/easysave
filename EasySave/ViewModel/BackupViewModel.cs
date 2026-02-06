using System.Collections.ObjectModel;
using EasySave.Model;
using EasySave.Service;

namespace EasySave.ViewModel;

public class BackupViewModel
{
    private BackupJobService _jobService { get; set; }
    private BackupExecutor _backupExecutor { get; set; }
    public ObservableCollection<BackupJob>? Jobs => _jobService.Jobs;
    
    public BackupViewModel(string appDirectory)
    {
        _jobService = new BackupJobService(appDirectory);
        _backupExecutor = new BackupExecutor();
    }

    public bool AddJob(BackupJob job) => _jobService.CreateJob(job);
    
    public bool DeleteJob(BackupJob job) => _jobService.DeleteJob(job);
    
    public bool ExecuteJob(BackupJob job, IProgressionObserver? progressionObserver = null)
    {
        job.State.AttachStateObserver(_jobService);
        if (progressionObserver != null) job.State.AttachProgressionObserver(progressionObserver);
        
        var result = _jobService.ExecuteJob(job);
        
        job.State.DetachStateObserver(_jobService);
        if (progressionObserver != null) job.State.DetachProgressionObserver(progressionObserver);

        return result;
    }

    public void UpdateJob(BackupJob job) => _jobService.UpdateJob(job);
    
    public Dictionary<int, bool> ExecuteJobsFromArgs(string? args)
    {
        var resultMap = new Dictionary<int, bool>();
        try
        {
            if (string.IsNullOrWhiteSpace(args)) return resultMap;

            var requestedIndices = new List<int>();
            var parts = args.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
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
                int jobIdx = idx - 1;
                if (Jobs != null && jobIdx >= 0 && jobIdx < Jobs.Count)
                {
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