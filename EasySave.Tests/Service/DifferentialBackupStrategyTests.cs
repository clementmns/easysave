using System.Text;
using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class DifferentialBackupStrategyTests
{
    public DifferentialBackupStrategyTests()
    {
        try { _ = SettingsService.GetInstance; }
        catch { SettingsService.Init(new TestsProperties()); }

        TransferLimitService.Instance.Initialize();
        SettingsService.GetInstance.SetCryptedExtensions([]);
        SettingsService.GetInstance.SetPriorityExtensions([]);
    }

    // ── New file is copied ────────────────────────────────────────────────────

    [Fact]
    public void Execute_NewFile_CopiesFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "new.txt"), "new content", Encoding.UTF8);

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Differential);
            var result = new DifferentialBackupStrategy().Execute(job);

            var backupFolder = Path.Combine(dst, Path.GetFileName(src));
            Assert.True(result);
            Assert.True(File.Exists(Path.Combine(backupFolder, "new.txt")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Unchanged file is skipped ─────────────────────────────────────────────

    [Fact]
    public void Execute_UnchangedFile_SkipsFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        var backupFolder = Path.Combine(dst, Path.GetFileName(src));
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(backupFolder);

        var srcFile = Path.Combine(src, "unchanged.txt");
        var dstFile = Path.Combine(backupFolder, "unchanged.txt");

        // Write the same content in both locations with identical timestamps.
        File.WriteAllText(srcFile, "same");
        File.WriteAllText(dstFile, "same");

        // Set destination timestamp to be the same or newer than source.
        var modTime = DateTime.Now.AddMinutes(-5);
        File.SetLastWriteTime(srcFile, modTime);
        File.SetLastWriteTime(dstFile, modTime); // identical → should skip

        var writtenBefore = File.GetLastWriteTime(dstFile);

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Differential);
            new DifferentialBackupStrategy().Execute(job);

            // File timestamp should remain the same – it was skipped.
            Assert.Equal(writtenBefore, File.GetLastWriteTime(dstFile));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Modified file is re-copied ────────────────────────────────────────────

    [Fact]
    public void Execute_ModifiedFile_OverwritesDestination()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        var backupFolder = Path.Combine(dst, Path.GetFileName(src));
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(backupFolder);

        var srcFile = Path.Combine(src, "modified.txt");
        var dstFile = Path.Combine(backupFolder, "modified.txt");

        File.WriteAllText(dstFile, "old content");
        File.WriteAllText(srcFile, "new content");

        // Ensure source is newer than destination.
        File.SetLastWriteTime(dstFile, DateTime.Now.AddHours(-1));
        File.SetLastWriteTime(srcFile, DateTime.Now);

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Differential);
            var result = new DifferentialBackupStrategy().Execute(job);

            Assert.True(result);
            Assert.Equal("new content", File.ReadAllText(dstFile, Encoding.UTF8));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Progression is set to 100 ─────────────────────────────────────────────

    [Fact]
    public void Execute_Directory_SetsProgressionTo100()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "f.txt"), "data");

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Differential);
            new DifferentialBackupStrategy().Execute(job);

            Assert.Equal(100, job.State.Progression);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Single file source: skip when unchanged ───────────────────────────────

    [Fact]
    public void Execute_SingleFile_SkipsWhenDestinationIsUpToDate()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var srcDir = Path.Combine(root, "srcDir");
        Directory.CreateDirectory(srcDir);
        var srcFile = Path.Combine(srcDir, "same.txt");
        File.WriteAllText(srcFile, "content");

        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(dst);
        var dstFile = Path.Combine(dst, "same.txt");
        File.WriteAllText(dstFile, "content");

        // Make destination newer than source.
        var t = DateTime.Now.AddHours(-2);
        File.SetLastWriteTime(srcFile, t);
        File.SetLastWriteTime(dstFile, t.AddMinutes(5));

        try
        {
            var job = new BackupJob(1, "J", srcFile, dst, BackupType.Differential);
            var result = new DifferentialBackupStrategy().Execute(job);

            Assert.True(result);
            Assert.Equal(100, job.State.Progression);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Missing source ────────────────────────────────────────────────────────

    [Fact]
    public void Execute_NonExistentSource_ThrowsFileNotFoundException()
    {
        var job = new BackupJob(1, "J", "/no/such/path", "/tmp/dst", BackupType.Differential);
        var strategy = new DifferentialBackupStrategy();

        Assert.Throws<FileNotFoundException>(() => strategy.Execute(job));
    }
}
