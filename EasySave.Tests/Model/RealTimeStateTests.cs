using EasySave.Model;

namespace EasySave.Tests.Model;

public class RealTimeStateTests
{
    private sealed class RecordingStateObserver : IRealTimeStateObserver
    {
        public int Updates { get; private set; }

        public void OnStateUpdated(RealTimeState state)
        {
            Updates++;
        }
    }

    private sealed class RecordingProgressionObserver : IProgressionObserver
    {
        public int ProgressionUpdates { get; private set; }
        public int LastProgression { get; private set; }

        public void OnProgressionUpdated(int progression)
        {
            ProgressionUpdates++;
            LastProgression = progression;
        }
    }

    [Fact]
    public void SetProperty_NotifiesObserversOncePerChange()
    {
        var state = new RealTimeState();
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);

        var start = observer.Updates;
        state.TotalFiles += 1;
        state.TotalFiles += 1;

        Assert.Equal(start + 2, observer.Updates);
    }

    [Fact]
    public void SetProgression_NotifiesProgressionObservers()
    {
        var state = new RealTimeState();
        var progressionObserver = new RecordingProgressionObserver();
        state.AttachProgressionObserver(progressionObserver);

        state.Progression = 50;
        
        Assert.Equal(1, progressionObserver.ProgressionUpdates);
        Assert.Equal(50, progressionObserver.LastProgression);
    }

    [Fact]
    public void SetProgression_DoesNotNotifyStateObservers()
    {
        var state = new RealTimeState();
        var stateObserver = new RecordingStateObserver();
        state.AttachStateObserver(stateObserver);

        var initialUpdates = stateObserver.Updates;
        state.Progression = 50;
        
        Assert.Equal(initialUpdates, stateObserver.Updates);
    }

    [Fact]
    public void SetSameValue_DoesNotNotifyObservers()
    {
        var state = new RealTimeState();
        var observer = new RecordingProgressionObserver();
        state.AttachProgressionObserver(observer);

        state.Progression = 10;
        var countAfterChange = observer.ProgressionUpdates;
        state.Progression = 10;

        Assert.Equal(countAfterChange, observer.ProgressionUpdates);
    }

    [Fact]
    public void Reset_SetsDefaultsAndNotifies()
    {
        var state = new RealTimeState
        {
            IsActive = true,
            Progression = 5,
            RemainingFiles = 3,
            RemainingFilesSize = 100,
            TotalFiles = 7,
            FileSize = 200
        };
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);

        state.Reset();

        Assert.False(state.IsActive);
        Assert.Equal(0, state.RemainingFiles);
        Assert.Equal(0, state.RemainingFilesSize);
        Assert.True(observer.Updates > 0);
    }

    [Fact]
    public void Detach_StopsNotifications()
    {
        var state = new RealTimeState();
        var observer = new RecordingProgressionObserver();
        state.AttachProgressionObserver(observer);
        state.Progression = 1;
        var countAfterAttach = observer.ProgressionUpdates;

        state.DetachProgressionObserver(observer);
        state.Progression = 2;

        Assert.Equal(countAfterAttach, observer.ProgressionUpdates);
    }
}
