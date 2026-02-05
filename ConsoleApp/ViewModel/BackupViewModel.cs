using System.Collections.ObjectModel;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Service;

namespace EasySave.ConsoleApp.ViewModel;

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
    
    public void ExecuteJob(BackupJob job)
    {
        job.State.Attach(_jobService);
        _jobService.ExecuteJob(job);
        job.State.Detach(_jobService);
    }

    public void UpdateJob(BackupJob job) => _jobService.UpdateJob(job);
    
    public bool ExecuteJobsFromArgs(string? args)
    {
        var selectedJobs = new List<BackupJob>();
        var parts = args?.Split(';').Select(p => p.Trim()).ToArray();

        if (parts != null)
        {
            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (!int.TryParse(range[0], out var start) || !int.TryParse(range[1], out var end)) continue;
                    if (Jobs == null) continue;
                    for (var i = start; i <= end && i <= Jobs.Count; i++)
                    {
                        selectedJobs.Add(Jobs[i - 1]);
                    }
                }
                else if (int.TryParse(part, out var jobNumber) && jobNumber > 0 && Jobs != null && jobNumber <= Jobs.Count)
                {
                    selectedJobs.Add(Jobs[jobNumber - 1]);
                }
            }
        }
        foreach (var job in selectedJobs)
        {
            ExecuteJob(job);
        }
        return true;
    }
}