using EasySave.Core.Model;

namespace EasySave.Tests.Model;

public class LogEntryTests
{
    // ── Default constructor ───────────────────────────────────────────────────

    [Fact]
    public void DefaultConstructor_CreatesEntryWithNullFields()
    {
        var entry = new LogEntry();

        Assert.Null(entry.Message);
        Assert.Null(entry.BackupName);
        Assert.Null(entry.SourcePath);
        Assert.Null(entry.DestinationPath);
        Assert.Equal(0, entry.FileSize);
        Assert.Null(entry.IsError);
        Assert.Null(entry.CopyDuration);
        Assert.Null(entry.CryptDuration);
    }

    // ── BackupJob constructor ─────────────────────────────────────────────────

    [Fact]
    public void JobConstructor_PopulatesBackupName()
    {
        var job = new BackupJob(1, "BackupA", "/src", "/dst", BackupType.Full);
        var entry = new LogEntry("test message", job);

        Assert.Equal("BackupA", entry.BackupName);
    }

    [Fact]
    public void JobConstructor_PopulatesMessage()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("hello world", job);

        Assert.Equal("hello world", entry.Message);
    }

    [Fact]
    public void JobConstructor_SetsTimestampToNow()
    {
        var before = DateTime.Now;
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("msg", job);
        var after = DateTime.Now;

        Assert.True(entry.Timestamp >= before && entry.Timestamp <= after);
    }

    [Fact]
    public void JobConstructor_IsErrorDefaultFalse()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("msg", job);

        Assert.False(entry.IsError);
    }

    [Fact]
    public void JobConstructor_IsErrorCanBeSetTrue()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("msg", job, isError: true);

        Assert.True(entry.IsError);
    }

    [Fact]
    public void JobConstructor_CopyDurationIsPreserved()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("msg", job, copyDuration: 42L);

        Assert.Equal(42L, entry.CopyDuration);
    }

    [Fact]
    public void JobConstructor_CryptDurationIsPreserved()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        var entry = new LogEntry("msg", job, cryptDuration: 100L);

        Assert.Equal(100L, entry.CryptDuration);
    }

    [Fact]
    public void JobConstructor_SourceAndDestinationAreUncPaths()
    {
        // When a valid absolute path is provided the UNC conversion should produce a non-null,
        // non-empty result.  We use Path.GetTempPath() which is always a valid absolute path.
        var src = Path.GetTempPath();
        var dst = Path.GetTempPath();

        var job = new BackupJob(1, "J", src, dst, BackupType.Full);
        var entry = new LogEntry("msg", job);

        // ConvertToUnc returns null only when conversion fails; for temp path it should succeed.
        Assert.False(string.IsNullOrWhiteSpace(entry.SourcePath));
        Assert.False(string.IsNullOrWhiteSpace(entry.DestinationPath));
    }

    [Fact]
    public void JobConstructor_FileSizeReflectsJobState()
    {
        var job = new BackupJob(1, "J", "/s", "/d", BackupType.Full);
        job.State.FileSize = 1234;

        var entry = new LogEntry("msg", job);

        Assert.Equal(1234, entry.FileSize);
    }
}
