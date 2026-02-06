using EasySave.Model;
using EasySave.Model.BackupStrategies;

namespace EasySave.Service;

public class BackupExecutor
{
    public bool ExecuteJob(BackupJob job)
    {
        var strategy = GetStrategy(job);
        strategy.Execute(job);
        return true;
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
