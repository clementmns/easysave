namespace EasySave.ConsoleApp.Model;

public class RealTimeState
{
    private readonly List<IRealTimeStateObserver> _observers = [];

    public DateTime LastUpdate
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsActive
    {
        get;
        set => SetField(ref field, value);
    }

    public int TotalFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public long FileSize
    {
        get;
        set => SetField(ref field, value);
    }

    public int Progression
    {
        get;
        set => SetField(ref field, value);
    }

    public int RemainingFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public long RemainingFilesSize
    {
        get;
        set => SetField(ref field, value);
    }

    public void Reset()
    {
        LastUpdate = DateTime.Now;
        IsActive = false;
        TotalFiles = 0;
        FileSize = 0;
        Progression = 0;
        RemainingFiles = 0;
        RemainingFilesSize = 0;
    }
    
    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.OnStateUpdated(this);
        }
    }

    private void SetField<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        NotifyObservers();
    }

    public void Attach(IRealTimeStateObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Detach(IRealTimeStateObserver observer)
    {
        _observers.Remove(observer);
    }

    public override string ToString()
    {
        return $"RealTimeState(LastUpdate={LastUpdate}, IsActive={IsActive}, TotalFiles={TotalFiles}, FileSize={FileSize}, Progression={Progression}, RemainingFiles={RemainingFiles}, RemainingFilesSize={RemainingFilesSize})";
    }
}
