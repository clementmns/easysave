namespace EasySave.Model;

public interface IBackupStrategy
{
    bool Execute(BackupJob job);
}