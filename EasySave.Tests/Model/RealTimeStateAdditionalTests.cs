using EasySave.Core.Model;

namespace EasySave.Tests.Model;

public class RealTimeStateAdditionalTests
{
    private sealed class RecordingStateObserver : IRealTimeStateObserver
    {
        public int Updates { get; private set; }
        public void OnStateUpdated(RealTimeState state) => Updates++;
    }

    // ── UpdateFileSize ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateFileSize_SetsAllRelatedFields()
    {
        var state = new RealTimeState();

        state.UpdateFileSize(500, 10);

        Assert.Equal(500, state.FileSize);
        Assert.Equal(10, state.TotalFiles);
        Assert.Equal(10, state.RemainingFiles);
        Assert.Equal(500, state.RemainingFilesSize);
    }

    [Fact]
    public void UpdateFileSize_NotifiesStateObservers()
    {
        var state = new RealTimeState();
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);

        var before = observer.Updates;
        state.UpdateFileSize(100, 5);

        Assert.True(observer.Updates > before);
    }

    // ── RefreshDisplay ────────────────────────────────────────────────────────

    [Fact]
    public void RefreshDisplay_NotifiesStateObservers()
    {
        var state = new RealTimeState();
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);

        var before = observer.Updates;
        state.RefreshDisplay();

        Assert.True(observer.Updates > before);
    }

    [Fact]
    public void RefreshDisplay_RaisesPropertyChangedForStatus()
    {
        var state = new RealTimeState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.RefreshDisplay();

        Assert.Contains(nameof(RealTimeState.Status), changed);
    }

    // ── Duplicate observer prevention ────────────────────────────────────────

    [Fact]
    public void AttachStateObserver_DoesNotAddDuplicate()
    {
        var state = new RealTimeState();
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);
        state.AttachStateObserver(observer); // second attach – should be ignored

        state.TotalFiles = 99;

        // The observer should only be called once per change, not twice.
        Assert.Equal(1, observer.Updates);
    }

    [Fact]
    public void DetachStateObserver_StopsNotifications()
    {
        var state = new RealTimeState();
        var observer = new RecordingStateObserver();
        state.AttachStateObserver(observer);
        state.TotalFiles = 1;
        var after = observer.Updates;

        state.DetachStateObserver(observer);
        state.TotalFiles = 2;

        Assert.Equal(after, observer.Updates);
    }

    // ── Status enum values ────────────────────────────────────────────────────

    [Fact]
    public void Status_DefaultIsReady()
    {
        var state = new RealTimeState();

        Assert.Equal(RealTimeState.RealTimeStatus.Ready, state.Status);
    }

    [Fact]
    public void Status_CanBeSetToEachValue()
    {
        var state = new RealTimeState();
        var allStatuses = Enum.GetValues<RealTimeState.RealTimeStatus>();

        foreach (var status in allStatuses)
        {
            state.Status = status;
            Assert.Equal(status, state.Status);
        }
    }

    // ── Reset ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Reset_ClearsCurrentFileName()
    {
        var state = new RealTimeState { CurrentFileName = "file.txt" };

        state.Reset();

        Assert.Equal(string.Empty, state.CurrentFileName);
    }

    [Fact]
    public void Reset_ClearsCurrentFileSize()
    {
        var state = new RealTimeState { CurrentFileSize = 999 };

        state.Reset();

        Assert.Equal(0, state.CurrentFileSize);
    }

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsAllKeyFields()
    {
        var state = new RealTimeState
        {
            TotalFiles = 5,
            Progression = 50,
            IsActive = true
        };

        var result = state.ToString();

        Assert.Contains("TotalFiles=5", result);
        Assert.Contains("Progression=50", result);
        Assert.Contains("IsActive=True", result);
    }
}
