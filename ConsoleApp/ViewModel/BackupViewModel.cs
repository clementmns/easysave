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
    
    public void ExecuteJob(BackupJob job) => _jobService.ExecuteJob(job);
    
    public void UpdateJob(BackupJob job) => _jobService.UpdateJob(job);
}