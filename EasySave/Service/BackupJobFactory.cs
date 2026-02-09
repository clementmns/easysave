using EasySave.Model;
using EasySave.Ressources;

namespace EasySave.Service;

public class BackupJobFactory
{
    private static BackupJobFactory? _instance;
    private const int MaxJobs = 5;

    private BackupJobFactory()
    {
        
    }

    public static BackupJobFactory GetInstance()
    {
        _instance ??= new BackupJobFactory();
        return _instance;
    }

    public BackupJob CreateJob(string name, string source, string destination, BackupType type, List<BackupJob>? existingJobs)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(Errors.NameCantBeNull);
        }

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentNullException(Errors.SourceCantBeNull);            
        }

        existingJobs ??= [];
        
        if (existingJobs.Count >= MaxJobs)
        {
            throw new Exception();
        }
        
        var newId = 0;
        for (var i = 1; i <= MaxJobs; i++)
        {
            // check if the id is already taken
            var isTaken = existingJobs.Any(job => job.Id == i);
            if (isTaken) continue;
            newId = i;
            break;
        }
        return new BackupJob(newId, name, source, destination, type); 
    }
}