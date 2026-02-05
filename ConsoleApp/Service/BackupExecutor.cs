using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Model.BackupStrategies;

namespace EasySave.ConsoleApp.Service;

public class BackupExecutor
{
    public bool ExecuteJob(BackupJob job)
    {
        try
        {
            // temps d'exécution
            IBackupStrategy strategy = GetStrategy(job);

            return strategy.Execute(job);
            
            // logger le temps d'execution
        }
        catch (Exception e)
        {
            // logger
            return false;
        }
        
    }

    public IBackupStrategy GetStrategy(BackupJob job)
    {
        return job.Type switch
        {
            BackupType.Full => new FullBackupStrategy(),
            BackupType.Differential => new DifferentialBackupStrategy(),
            _ => throw new InvalidOperationException("Backup type not supported")
        };
    }
}