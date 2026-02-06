namespace EasySave.Model;

public interface IRealTimeStateObserver
{
    void OnStateUpdated(RealTimeState state);
}