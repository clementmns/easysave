namespace EasySave.Model;

/// <summary>
/// Real time state of a backup job.
/// </summary>
public class RealTimeState
{
    private readonly List<IRealTimeStateObserver> _stateObservers = [];
    private readonly List<IProgressionObserver> _progressionObservers = [];

    public DateTime LastUpdate
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
        set => SetFieldProgression(ref field, value);
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
    
    private void NotifyStateObservers()
    {
        foreach (var observer in _stateObservers)
        {
            observer.OnStateUpdated(this);
        }
    }
    
    private void NotifyProgressionObservers()
    {
        foreach (var observer in _progressionObservers)
        {
            observer.OnProgressionUpdated(Progression);
        }
    }

    private void SetField<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        NotifyStateObservers();
    }
    
    private void SetFieldProgression<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        
        field = value;
        NotifyProgressionObservers();
    }

    public void AttachStateObserver(IRealTimeStateObserver observer)
    {
        if (!_stateObservers.Contains(observer))
        {
            _stateObservers.Add(observer);
        }
    }

    public void DetachStateObserver(IRealTimeStateObserver observer)
    {
        _stateObservers.Remove(observer);
    }

    public void AttachProgressionObserver(IProgressionObserver observer)
    {
        if (!_progressionObservers.Contains(observer))
        {
            _progressionObservers.Add(observer);
        }
    }

    public void DetachProgressionObserver(IProgressionObserver observer)
    {
        _progressionObservers.Remove(observer);
    }

    public override string ToString()
    {
        return $"RealTimeState(LastUpdate={LastUpdate}, IsActive={IsActive}, TotalFiles={TotalFiles}, FileSize={FileSize}, Progression={Progression}, RemainingFiles={RemainingFiles}, RemainingFilesSize={RemainingFilesSize})";
    }
}
