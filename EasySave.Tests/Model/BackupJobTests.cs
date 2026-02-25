using System.ComponentModel;
using EasySave.Core.Model;

namespace EasySave.Tests.Model;

public class BackupJobTests
{
    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var job = new BackupJob(3, "MyJob", "/src", "/dst", BackupType.Full);

        Assert.Equal(3, job.Id);
        Assert.Equal("MyJob", job.Name);
        Assert.Equal("/src", job.SourcePath);
        Assert.Equal("/dst", job.DestinationPath);
        Assert.Equal(BackupType.Full, job.Type);
        Assert.NotNull(job.State);
    }

    [Fact]
    public void DefaultConstructor_CreatesJobWithDefaultValues()
    {
        var job = new BackupJob();

        Assert.Equal(0, job.Id);
        Assert.Equal(string.Empty, job.Name);
        Assert.Equal(string.Empty, job.SourcePath);
        Assert.Equal(string.Empty, job.DestinationPath);
        Assert.NotNull(job.State);
        Assert.NotNull(job.PauseGate);
        Assert.NotNull(job.CancellationTokenSource);
    }

    [Fact]
    public void Constructor_Differential_SetsType()
    {
        var job = new BackupJob(1, "DiffJob", "/a", "/b", BackupType.Differential);

        Assert.Equal(BackupType.Differential, job.Type);
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    [Fact]
    public void SetName_RaisesPropertyChanged()
    {
        var job = new BackupJob(1, "OldName", "/src", "/dst", BackupType.Full);
        var raised = new List<string?>();
        job.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        job.Name = "NewName";

        Assert.Contains(nameof(BackupJob.Name), raised);
    }

    [Fact]
    public void SetSameName_DoesNotRaisePropertyChanged()
    {
        var job = new BackupJob(1, "Same", "/src", "/dst", BackupType.Full);
        var count = 0;
        job.PropertyChanged += (_, _) => count++;

        job.Name = "Same";

        Assert.Equal(0, count);
    }

    [Fact]
    public void SetSourcePath_RaisesPropertyChanged()
    {
        var job = new BackupJob(1, "J", "/old", "/dst", BackupType.Full);
        string? changed = null;
        job.PropertyChanged += (_, e) => changed = e.PropertyName;

        job.SourcePath = "/new";

        Assert.Equal(nameof(BackupJob.SourcePath), changed);
    }

    [Fact]
    public void SetId_RaisesPropertyChanged()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        string? changed = null;
        job.PropertyChanged += (_, e) => changed = e.PropertyName;

        job.Id = 99;

        Assert.Equal(nameof(BackupJob.Id), changed);
    }

    // ── ResetCancellation ────────────────────────────────────────────────────

    [Fact]
    public void ResetCancellation_ReplacesToken()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var originalToken = job.CancellationTokenSource;
        originalToken.Cancel();

        job.ResetCancellation();

        Assert.NotSame(originalToken, job.CancellationTokenSource);
        Assert.False(job.CancellationTokenSource.IsCancellationRequested);
    }

    [Fact]
    public void ResetCancellation_OpensPauseGate()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        job.PauseGate.Reset(); // close the gate

        job.ResetCancellation();

        Assert.True(job.PauseGate.IsSet);
    }

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsAllKeyFields()
    {
        var job = new BackupJob(7, "TestJob", "/src/path", "/dst/path", BackupType.Differential);

        var result = job.ToString();

        Assert.Contains("7", result);
        Assert.Contains("TestJob", result);
        Assert.Contains("/src/path", result);
        Assert.Contains("/dst/path", result);
        Assert.Contains("Differential", result);
    }
}
