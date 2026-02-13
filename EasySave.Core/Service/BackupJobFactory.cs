using EasySave.Core.Model;
using EasySave.Core.Resources;

namespace EasySave.Core.Service;

public class BackupJobFactory
{
    private static BackupJobFactory? _instance;

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
        
        if (existingJobs.Count >= SettingsService.GetInstance.Settings.MaxJobs)
        {
            throw new InvalidOperationException(Errors.MaxJobsReached);
        }
        
        var newId = 0;
        var maxId = existingJobs.Select(job => job.Id).DefaultIfEmpty(0).Max() + 1; // get one more than the max id in the list to avoid memory leap
        for (var i = 1; i <= maxId; i++)
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
