using EasySave.Model;
using EasySave.Service;

namespace EasySave.Tests.Service;

public class BackupJobFactoryTests
{
    [Fact]
    public void CreateJob_AssignsFirstAvailableId()
    {
        var factory = BackupJobFactory.GetInstance();
        var existing = new List<BackupJob>
        {
            new(1, "Job1", "src1", "dst1", BackupType.Full),
            new(3, "Job3", "src3", "dst3", BackupType.Differential)
        };

        var job = factory.CreateJob("New", "src", "dst", BackupType.Full, existing);

        Assert.Equal(2, job.Id);
        Assert.Equal("New", job.Name);
        Assert.Equal("src", job.SourcePath);
        Assert.Equal("dst", job.DestinationPath);
        Assert.Equal(BackupType.Full, job.Type);
    }

    [Fact]
    public void CreateJob_ThrowsWhenNameIsEmpty()
    {
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("", "src", "dst", BackupType.Full, []));
    }

    [Fact]
    public void CreateJob_ThrowsWhenSourceOrDestinationIsEmpty()
    {
        var factory = BackupJobFactory.GetInstance();

        Assert.Throws<ArgumentNullException>(() =>
            factory.CreateJob("Job", "", "dst", BackupType.Full, []));
    }

    [Fact]
    public void CreateJob_ThrowsWhenMaxJobsReached()
    {
        var factory = BackupJobFactory.GetInstance();
        var existing = new List<BackupJob>
        {
            new(1, "J1", "src", "dst", BackupType.Full),
            new(2, "J2", "src", "dst", BackupType.Full),
            new(3, "J3", "src", "dst", BackupType.Full),
            new(4, "J4", "src", "dst", BackupType.Full),
            new(5, "J5", "src", "dst", BackupType.Full)
        };

        Assert.Throws<Exception>(() =>
            factory.CreateJob("New", "src", "dst", BackupType.Full, existing));
    }
}
