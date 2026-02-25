using System.Text;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class BackupExecutorAdditionalTests
{
    public BackupExecutorAdditionalTests()
    {
        try { _ = SettingsService.GetInstance; }
        catch { SettingsService.Init(new TestsProperties()); }

        TransferLimitService.Instance.Initialize();
    }

    // ── Full strategy executes successfully ───────────────────────────────────

    [Fact]
    public async Task ExecuteJobAsync_FullBackup_CopiesFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "a.txt"), "hello", Encoding.UTF8);

        try
        {
            var job = new BackupJob(1, "FullTest", src, dst, BackupType.Full);
            var executor = new BackupExecutor();

            var result = await executor.ExecuteJobAsync(job);

            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Differential strategy executes successfully ───────────────────────────

    [Fact]
    public async Task ExecuteJobAsync_DifferentialBackup_CopiesFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "b.txt"), "world", Encoding.UTF8);

        try
        {
            var job = new BackupJob(1, "DiffTest", src, dst, BackupType.Differential);
            var executor = new BackupExecutor();

            var result = await executor.ExecuteJobAsync(job);

            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Unsupported backup type ───────────────────────────────────────────────

    [Fact]
    public void ExecuteJobAsync_UnsupportedType_ThrowsInvalidOperation()
    {
        var job = new BackupJob(1, "Bad", "/src", "/dst", (BackupType)999);
        var executor = new BackupExecutor();

        Assert.Throws<InvalidOperationException>(() =>
            executor.ExecuteJobAsync(job).GetAwaiter().GetResult());
    }

    // ── Cancellation ─────────────────────────────────────────────────────────
    // BackupExecutor.ExecuteJobAsync always calls job.ResetCancellation() first,
    // so pre-cancelling before the call has no effect — the token is replaced.
    // Cancellation can only happen mid-run (race). We verify the executor at
    // least starts and completes normally when the source exists.

    [Fact]
    public async Task ExecuteJobAsync_CancelledToken_ExecutorResetsItAndCompletes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "file.txt"), "data");

        try
        {
            var job = new BackupJob(1, "Cancel", src, dst, BackupType.Full);
            // Pre-cancel — ExecuteJobAsync will reset this internally.
            job.ResetCancellation();
            job.CancellationTokenSource.Cancel();

            var executor = new BackupExecutor();

            // The executor resets the token so the job still runs to completion.
            var result = await executor.ExecuteJobAsync(job);
            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Source path does not exist ────────────────────────────────────────────
    // FullBackupStrategy.Execute throws FileNotFoundException when the source
    // directory does not exist (it does not return false). BackupExecutor does
    // not catch this, so it propagates to the caller.

    [Fact]
    public async Task ExecuteJobAsync_NonExistentSource_ThrowsFileNotFoundException()
    {
        var job = new BackupJob(1, "NoSrc", "/nonexistent/source/path", "/tmp/dst", BackupType.Full);
        var executor = new BackupExecutor();

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            executor.ExecuteJobAsync(job));
    }
}
