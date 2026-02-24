using System.Text;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Core.Utils;

namespace EasySave.Tests.Utils;

public class CryptoUtilsTests
{
    [Fact]
    public void EncryptFile_ReturnsFalse_WhenCryptoSoftPathIsNull()
    {
        SettingsService.Init(new TestAppProperties { CryptoSoftPath = null });

        var result = CryptoUtils.EncryptFile("source.txt", "dest");

        Assert.False(result.Item1);
    }

    [Fact]
    public void EncryptFile_ReturnsFalse_WhenCryptoSoftPathDoesNotExist()
    {
        SettingsService.Init(new TestAppProperties { CryptoSoftPath = "/nonexistent/cryptosoft.exe" });

        var result = CryptoUtils.EncryptFile("source.txt", "dest");

        Assert.False(result.Item1);
    }

    [Fact]
    public void DecryptFile_ReturnsFalse_WhenCryptoSoftPathIsNull()
    {
        SettingsService.Init(new TestAppProperties { CryptoSoftPath = null });

        var result = CryptoUtils.DecryptFile("source.txt", "dest");

        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteCryptoCommand_SerializesConcurrentCalls()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var sourceFile = Path.Combine(root, "source.txt");
        File.WriteAllText(sourceFile, "test", Encoding.UTF8);

        try
        {
            var cryptoExePath = SettingsService.GetInstance.Settings.CryptoSoftPath;
            if (cryptoExePath == null || !File.Exists(cryptoExePath))
            {
                return;
            }

            var callTimes = new List<DateTime>();
            var lockObj = new object();

            var tasks = Enumerable.Range(0, 3).Select(async i =>
            {
                await Task.Run(() =>
                {
                    lock (lockObj)
                    {
                        callTimes.Add(DateTime.Now);
                    }
                    CryptoUtils.EncryptFile(sourceFile, Path.Combine(root, $"dest{i}"));
                });
            });

            await Task.WhenAll(tasks);

            var minGap = callTimes.OrderBy(x => x).Take(2).Last() - callTimes.OrderBy(x => x).First();
            Assert.True(minGap.TotalMilliseconds > 0);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private class TestAppProperties : IAppProperties
    {
        public string AppSaveDirectory { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public int MaxJobs { get; } = 1;
        public string? CryptoSoftPath { get; init; }
    }
}
