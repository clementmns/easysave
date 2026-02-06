namespace EasySave.ConsoleApp.Model;

public class RealTimeState
{
    private readonly List<IRealTimeStateObserver> _observers = [];

    private DateTime LastUpdate
    {
        get;
        set => SetField(ref field, value);
    } = DateTime.Now;

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
        set => SetFieldProgressBar(ref field, value);
    }

    public long RemainingFiles
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
    
    private void ProgressNotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.ProgressBarUpdate(this.Progression);
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
    
    private void SetFieldProgressBar<T>(ref T field, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            ProgressNotifyObservers();
        }
    }
}
