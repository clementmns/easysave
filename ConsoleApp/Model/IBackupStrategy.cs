namespace EasySave.ConsoleApp.Model;

public interface IBackupStrategy
{
    bool Execute(BackupJob job);
}