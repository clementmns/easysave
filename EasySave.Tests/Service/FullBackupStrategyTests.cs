using System.Text;
using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class FullBackupStrategyTests
{
    public FullBackupStrategyTests()
    {
        try { _ = SettingsService.GetInstance; }
        catch { SettingsService.Init(new TestsProperties()); }

        TransferLimitService.Instance.Initialize();
        SettingsService.GetInstance.SetCryptedExtensions([]);
        SettingsService.GetInstance.SetPriorityExtensions([]);
    }

    // ── Single-file source ────────────────────────────────────────────────────

    [Fact]
    public void Execute_SingleFile_CopiesFileToDestination()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source", "file.txt");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        Directory.CreateDirectory(dst);
        File.WriteAllText(src, "hello", Encoding.UTF8);

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Full);
            var strategy = new FullBackupStrategy();

            var result = strategy.Execute(job);

            var expectedDest = Path.Combine(dst, "file.txt");
            Assert.True(result);
            Assert.True(File.Exists(expectedDest));
            Assert.Equal("hello", File.ReadAllText(expectedDest, Encoding.UTF8));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Execute_SingleFile_SetsProgressionTo100()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "s", "f.txt");
        var dst = Path.Combine(root, "d");
        Directory.CreateDirectory(Path.GetDirectoryName(src)!);
        Directory.CreateDirectory(dst);
        File.WriteAllText(src, "data");

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Full);
            new FullBackupStrategy().Execute(job);

            Assert.Equal(100, job.State.Progression);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Directory source ──────────────────────────────────────────────────────

    [Fact]
    public void Execute_Directory_CopiesAllFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var sub = Path.Combine(src, "sub");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(sub);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "a.txt"), "aaa");
        File.WriteAllText(Path.Combine(sub, "b.txt"), "bbb");

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Full);
            var result = new FullBackupStrategy().Execute(job);

            var backupFolder = Path.Combine(dst, Path.GetFileName(src));
            Assert.True(result);
            Assert.True(File.Exists(Path.Combine(backupFolder, "a.txt")));
            Assert.True(File.Exists(Path.Combine(backupFolder, "sub", "b.txt")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Execute_Directory_SetsProgressionTo100()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "x.txt"), "x");

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Full);
            new FullBackupStrategy().Execute(job);

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
        var job = new BackupJob(1, "J", "/no/such/path", "/tmp/dst", BackupType.Full);
        var strategy = new FullBackupStrategy();

        Assert.Throws<FileNotFoundException>(() => strategy.Execute(job));
    }

    // ── Priority files are copied first ──────────────────────────────────────

    [Fact]
    public void Execute_Directory_WithPriorityExtension_CopiesPriorityFilesFirst()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "source");
        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(dst);
        File.WriteAllText(Path.Combine(src, "important.docx"), "important");
        File.WriteAllText(Path.Combine(src, "normal.txt"), "normal");

        SettingsService.GetInstance.SetPriorityExtensions([".docx"]);

        try
        {
            var job = new BackupJob(1, "J", src, dst, BackupType.Full);
            var result = new FullBackupStrategy().Execute(job);

            var backupFolder = Path.Combine(dst, Path.GetFileName(src));
            Assert.True(result);
            Assert.True(File.Exists(Path.Combine(backupFolder, "important.docx")));
            Assert.True(File.Exists(Path.Combine(backupFolder, "normal.txt")));
        }
        finally
        {
            SettingsService.GetInstance.SetPriorityExtensions([]);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
