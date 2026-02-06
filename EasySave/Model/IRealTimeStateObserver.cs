namespace EasySave.Model;

/// <summary>
/// Interface for observing real time state changes of a backup job.
/// </summary>
public interface IRealTimeStateObserver
{
    void OnStateUpdated(RealTimeState state);
}