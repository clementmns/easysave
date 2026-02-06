namespace EasySave.ConsoleApp.Model;

public interface IRealTimeStateObserver
{
    void OnStateUpdated(RealTimeState state);
}