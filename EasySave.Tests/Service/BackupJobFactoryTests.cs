using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class BackupJobFactoryTests
{
    [Fact]
    public void CreateJob_ThrowsWhenNameIsEmpty()
    {
        SettingsService.Init(new TestsProperties());
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("", "src", "dst", BackupType.Full, []));
    }

    [Fact]
    public void CreateJob_ThrowsWhenSourceOrDestinationIsEmpty()
    {
        SettingsService.Init(new TestsProperties());
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("Job", "", "dst", BackupType.Full, []));
    }

    [Fact]
    public void CreateJob_ThrowsWhenMaxJobsReached()
    {
        SettingsService.Init(new TestsProperties());
        var factory = BackupJobFactory.GetInstance();
        var existing = new List<BackupJob>
        {
            new(1, "J1", "src", "dst", BackupType.Full),
            new(2, "J2", "src", "dst", BackupType.Full),
            new(3, "J3", "src", "dst", BackupType.Full),
            new(4, "J4", "src", "dst", BackupType.Full),
            new(5, "J5", "src", "dst", BackupType.Full)
        };

        Assert.Throws<InvalidOperationException>(() =>
            factory.CreateJob("New", "src", "dst", BackupType.Full, existing));
    }
}
