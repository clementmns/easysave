using System.Threading.Tasks;
using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;

namespace EasySave.Core.Service;

public class BackupExecutor
{
    public async Task<bool> ExecuteJobAsync(BackupJob job)
    {
        return await Task.Run(() =>
        {
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
            _ => throw new InvalidOperationException("Backup type not supported")
        };
    }
}
