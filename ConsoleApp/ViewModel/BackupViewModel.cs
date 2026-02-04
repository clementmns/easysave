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

    public void AddJob(string name, string source, string destination, BackupType type)
    {
        try
        {
            var currentJobs = Jobs?.ToList() ?? new List<BackupJob>();
            var factory = BackupJobFactory.GetInstance();
            var newJob = factory.CreateJob(name, source, destination, type, currentJobs);
            _jobService.CreateJob(newJob);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void DeleteJob(BackupJob job) => _jobService.DeleteJob(job);
    
    public void ExecuteJob(BackupJob job) => _jobService.ExecuteJob(job);
    
    public void UpdateJob(BackupJob job) => _jobService.UpdateJob(job);
}