using EasySave.Model;
using EasySave.Model.BackupStrategies;

namespace EasySave.Service;

public class BackupExecutor
{
    public void ExecuteJob(BackupJob job)
    {
        IBackupStrategy strategy = GetStrategy(job);
        strategy.Execute(job);
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
