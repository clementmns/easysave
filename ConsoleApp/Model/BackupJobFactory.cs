namespace EasySave.ConsoleApp.Model;

public class BackupJobFactory
{
    private static BackupJobFactory? _instance;

    private BackupJobFactory()
    {
        
    }

    public static BackupJobFactory GetInstance()
    {
        if (_instance == null)
        {
            _instance = new BackupJobFactory();
        }

        return _instance;
    }

    public BackupJob CreateJob(string name, string source, string destination, BackupType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException("The name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentNullException("The source and destination cannot be null or empty.");            
        }
        
        return new BackupJob(0, name, source, destination, type); //Gérer l'id ???
    }
}