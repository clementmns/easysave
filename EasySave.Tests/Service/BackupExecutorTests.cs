using EasySave.Core.Model;
using EasySave.Core.Service;

namespace EasySave.Tests.Service;

public class BackupExecutorTests
{
    [Fact]
    public void ExecuteJob_ThrowsWhenBackupTypeUnsupported()
    {
        var executor = new BackupExecutor();
        var job = new BackupJob(1, "Job", "src", "dst", (BackupType)999);

        Assert.Throws<InvalidOperationException>(() => executor.ExecuteJobAsync(job).GetAwaiter().GetResult());
    }
}
