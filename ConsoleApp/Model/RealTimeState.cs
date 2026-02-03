namespace EasySave.ConsoleApp.Model;

public class RealTimeState
{
    public string? Name { get; set; }
    public DateTime LastUpdate { get; set; }
    public bool IsActive { get; set; }
    public int TotalFiles { get; set; }
    public long FileSize { get; set; }
    public int Progression { get; set; }
    public int RemainingFiles { get; set; }
    public long RemainingFilesSize { get; set; }
    public string? SourceDirectory { get; set; }
    public string? DestinationDirectory { get; set; }

    private List<IBackupUpdatedEvent> _observers = new List<IBackupUpdatedEvent>();
    
    public void Attach(IBackupUpdatedEvent observer)
    {
        _observers.Add(observer);
    }

    public void Detach(IBackupUpdatedEvent observer)
    {
        _observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (var observer in _observers)
        {
            observer.Update(this);
        }
    }
}