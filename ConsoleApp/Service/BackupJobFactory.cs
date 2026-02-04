using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Ressources;

namespace EasySave.ConsoleApp.Service;

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

    public BackupJob CreateJob(string name, string source, string destination, BackupType type, List<BackupJob> existingJobs)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(Errors.NameCantBeNull);
        }

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentNullException(Errors.SourceCantBeNull);            
        }

        if (existingJobs == null)
        {
            existingJobs = new List<BackupJob>();
        }
        
        if (existingJobs.Count >= MaxJobs)
        {
            throw new Exception();
        }
        
        int newId = 0;
        for (int i = 1; i <= MaxJobs; i++)
        {
            bool isTaken = existingJobs.Any(job => job.Id == i);
            if (!isTaken)
            {
                newId = i;
                break;
            }
        }
        return new BackupJob(newId, name, source, destination, type); 
    }
}