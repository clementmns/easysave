namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class DifferentialBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        var result = (File.Exists(job.SourcePath), Directory.Exists(job.DestinationPath)) switch
        {
            (true, false) => ProcessFile(job),
            (false, true) => ProcessDirectory(job),
            _ => throw new FileNotFoundException(Ressources.Errors.ProcessingError)
        };
        
        return result;
    }

    private static bool ProcessFile(BackupJob job)
    {
        
    }

    private static bool ProcessDirectory(BackupJob job)
    {
        
    }
}