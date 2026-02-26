using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class BackupJobFactoryAdditionalTests
{
    public BackupJobFactoryAdditionalTests()
    {
        SettingsService.Init(new TestsProperties());
    }

    // ── ID assignment ─────────────────────────────────────────────────────────

    [Fact]
    public void CreateJob_WithEmptyList_AssignsIdOne()
    {
        var factory = BackupJobFactory.GetInstance();

        var job = factory.CreateJob("J", "/src", "/dst", BackupType.Full, []);

        Assert.Equal(1, job.Id);
    }

    [Fact]
    public void CreateJob_WithNullExistingJobs_AssignsIdOne()
    {
        var factory = BackupJobFactory.GetInstance();

        var job = factory.CreateJob("J", "/src", "/dst", BackupType.Full, null);

        Assert.Equal(1, job.Id);
    }

    [Fact]
    public void CreateJob_WithExistingIds_AssignsNextAvailableId()
    {
        var factory = BackupJobFactory.GetInstance();
        var existing = new List<BackupJob>
        {
            new(1, "J1", "/s", "/d", BackupType.Full),
            new(2, "J2", "/s", "/d", BackupType.Full)
        };

        var job = factory.CreateJob("New", "/src", "/dst", BackupType.Full, existing);

        Assert.Equal(3, job.Id);
    }

    [Fact]
    public void CreateJob_FillsIdGap()
    {
        // IDs 1 and 3 exist – the factory should re-use ID 2 (the gap).
        var factory = BackupJobFactory.GetInstance();
        var existing = new List<BackupJob>
        {
            new(1, "J1", "/s", "/d", BackupType.Full),
            new(3, "J3", "/s", "/d", BackupType.Full)
        };

        var job = factory.CreateJob("Gap", "/src", "/dst", BackupType.Full, existing);

        Assert.Equal(2, job.Id);
    }

    // ── Property propagation ──────────────────────────────────────────────────

    [Fact]
    public void CreateJob_SetsNameCorrectly()
    {
        var factory = BackupJobFactory.GetInstance();

        var job = factory.CreateJob("MyBackup", "/src", "/dst", BackupType.Differential, []);

        Assert.Equal("MyBackup", job.Name);
        Assert.Equal("/src", job.SourcePath);
        Assert.Equal("/dst", job.DestinationPath);
        Assert.Equal(BackupType.Differential, job.Type);
    }

    // ── Validation: whitespace-only inputs ───────────────────────────────────

    [Fact]
    public void CreateJob_ThrowsWhenNameIsWhitespace()
    {
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("   ", "/src", "/dst", BackupType.Full, []));
    }

    [Fact]
    public void CreateJob_ThrowsWhenDestinationIsEmpty()
    {
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("J", "/src", "", BackupType.Full, []));
    }
}
