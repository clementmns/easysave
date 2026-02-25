using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Core.Model;

/// <summary>
/// Real time state of a backup job.
/// </summary>
public class RealTimeState : INotifyPropertyChanged
{
    public enum RealTimeStatus
    {
        Ready,
        Done,
        Error,
        OnGoing,
        Paused
    }

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

    public RealTimeStatus Status
    {
        get;
        set => SetField(ref field, value);
    } = RealTimeStatus.Ready;

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

    public string CurrentFileName
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public long CurrentFileSize
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
        CurrentFileName = string.Empty;
        CurrentFileSize = 0;
    }

    public void UpdateFileSize(long fileSize, int totalFiles)
    {
        FileSize = fileSize;
        TotalFiles = totalFiles;
        RemainingFiles = totalFiles;
        RemainingFilesSize = fileSize;
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
        var observers = _progressionObservers.ToList();
        foreach (var observer in observers)
        {
            observer.OnProgressionUpdated(Progression);
        }
    }
    
    public void RefreshDisplay()
    {
        NotifyStateObservers();
        OnPropertyChanged(nameof(Status));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        NotifyStateObservers();
        OnPropertyChanged(propertyName);
    }
    
    private void SetFieldProgression<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        
        field = value;
        NotifyProgressionObservers();
        OnPropertyChanged(propertyName);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"RealTimeState(LastUpdate={LastUpdate}, IsActive={IsActive}, TotalFiles={TotalFiles}, FileSize={FileSize}, Progression={Progression}, RemainingFiles={RemainingFiles}, RemainingFilesSize={RemainingFilesSize}, CurrentFileName={CurrentFileName}, CurrentFileSize={CurrentFileSize})";
    }
}