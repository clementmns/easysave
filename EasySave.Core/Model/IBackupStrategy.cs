namespace EasySave.Core.Model;

/// <summary>
/// Interface for backup strategies.
/// </summary>
public interface IBackupStrategy
{
    bool Execute(BackupJob job); 
    Task ExecuteAsync(BackupJob job, List<string> priorityExtensions);
}

