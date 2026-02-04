namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class DifferentialBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        return true; // TODO
    }
}