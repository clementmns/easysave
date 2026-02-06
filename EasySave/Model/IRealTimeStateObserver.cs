namespace EasySave.Model;

public interface IRealTimeStateObserver
{
    void OnStateUpdated(RealTimeState state);
    void ProgressBarUpdate(int progresion);
}