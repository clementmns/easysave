namespace EasySave.ConsoleApp.Model;

public interface IBackupUpdatedEvent
{
    void Update(object state);
}