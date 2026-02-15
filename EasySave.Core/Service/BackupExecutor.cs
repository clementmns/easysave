using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;
using EasyLog;

namespace EasySave.Core.Service;

public class BackupExecutor
{
    public bool ExecuteJob(BackupJob job)
    {
        // Check if business software is running
        if (ProcessMonitorService.Instance.IsBusinessSoftwareRunning)
        {
            Logger.Instance.Write($"Backup blocked for job {job.Name}: business software detected");
            return false;
        }
        
        var strategy = GetStrategy(job);
        return strategy.Execute(job);
    }

    private static IBackupStrategy GetStrategy(BackupJob job)
    {
        return job.Type switch
        {
            BackupType.Full => new FullBackupStrategy(),
            BackupType.Differential => new DifferentialBackupStrategy(),
            _ => throw new InvalidOperationException("Backup type not supported")
        };
    }
}
