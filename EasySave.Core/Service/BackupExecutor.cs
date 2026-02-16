using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;
using EasyLog;
using EasySave.Core.Resources;

namespace EasySave.Core.Service;

public class BackupExecutor
{
    public async Task<bool> ExecuteJobAsync(BackupJob job)
    {
        return await Task.Run(() =>
        {
            // Check if business software is running
            if (ProcessMonitorService.Instance.IsBusinessSoftwareRunning)
            {
                Logger.Instance.Write(new LogEntry(Errors.BackupBlocked, job, isError: true));
                return false;
            }
            
            var strategy = GetStrategy(job);
            return strategy.Execute(job);
        });
    }

    private static IBackupStrategy GetStrategy(BackupJob job)
    {
        return job.Type switch
        {
            BackupType.Full => new FullBackupStrategy(),
            BackupType.Differential => new DifferentialBackupStrategy(),
            _ => throw new InvalidOperationException(Errors.UnknownBackupType)
        };
    }
}
