using EasySave.Model;

namespace EasySave.Tests.Model;

public class RealTimeStateTests
{
    private sealed class RecordingObserver : IRealTimeStateObserver
    {
        public int Updates { get; private set; }

        public void OnStateUpdated(RealTimeState state)
        {
            Updates++;
        }

        public void ProgressBarUpdate(int progression)
        {
            return;
        }
    }

    [Fact]
    public void SetProperty_NotifiesObserversOncePerChange()
    {
        var state = new RealTimeState();
        var observer = new RecordingObserver();
        state.Attach(observer);

        var start = observer.Updates;
        state.TotalFiles += 1;
        state.TotalFiles += 1;

        Assert.Equal(start + 2, observer.Updates);
    }

    [Fact]
    public void SetSameValue_DoesNotNotifyObservers()
    {
        var state = new RealTimeState();
        var observer = new RecordingObserver();
        state.Attach(observer);

        state.Progression = 10;
        var countAfterChange = observer.Updates;
        state.Progression = 10;

        Assert.Equal(countAfterChange, observer.Updates);
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
        var observer = new RecordingObserver();
        state.Attach(observer);

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
        var observer = new RecordingObserver();
        state.Attach(observer);
        state.Progression = 1;
        var countAfterAttach = observer.Updates;

        state.Detach(observer);
        state.Progression = 2;

        Assert.Equal(countAfterAttach, observer.Updates);
    }
}
