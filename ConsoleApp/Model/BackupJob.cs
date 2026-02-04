namespace EasySave.ConsoleApp.Model;

public class BackupJob
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SourcePath { get; set; }
    public string DestinationPath { get; set; }
    public BackupType Type { get; set; }
    public RealTimeState State { get; set; }

    public BackupJob(int id, string name, string sourcePath, string destinationPath, BackupType type)
    {
        Id = id;
        Name = name;
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        Type = type;
        State = new RealTimeState();
    }

    public override string ToString()
    {
        return $"BackupJob(Id={Id}, Name={Name}, SourcePath={SourcePath}, DestinationPath={DestinationPath}, Type={Type}, State={State})";
    }
}