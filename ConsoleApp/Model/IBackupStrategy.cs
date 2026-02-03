namespace EasySave.ConsoleApp.Model;

public interface IBackupStrategy
{
    void Execute(BackupJob job);
}