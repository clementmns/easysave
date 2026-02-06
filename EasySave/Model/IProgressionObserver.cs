namespace EasySave.Model;

/// <summary>
/// Interface for observing backup progression changes.
/// </summary>
public interface IProgressionObserver
{
    void OnProgressionUpdated(int progression);
}
