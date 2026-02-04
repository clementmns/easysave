using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Ressources;

namespace EasySave.ConsoleApp.Service;

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

    public BackupJob CreateJob(string name, string source, string destination, BackupType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(Errors.NameCantBeNull);
        }

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentNullException(Errors.SourceCantBeNull);            
        }
        
        return new BackupJob(0, name, source, destination, type); //Gérer l'id ???
    }
}