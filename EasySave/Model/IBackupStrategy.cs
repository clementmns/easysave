namespace EasySave.Model;

/// <summary>
/// Interface for backup strategies.
/// </summary>
public interface IBackupStrategy
{
    bool Execute(BackupJob job);
}