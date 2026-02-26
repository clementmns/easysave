using EasyLog.LoggerStrategies;

namespace EasySave.Tests.EasyLog;

public class JsonLoggerStrategyTests
{
    // ── LocalWrite creates file ───────────────────────────────────────────────

    [Fact]
    public void LocalWrite_CreatesJsonFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var logPath = Path.Combine(root, "log");

        try
        {
            var strategy = new JsonLoggerStrategy();
            strategy.LocalWrite(new { Message = "hello" }, logPath);

            Assert.True(File.Exists(logPath + ".json"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LocalWrite_ContentIsValidJson()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var logPath = Path.Combine(root, "log");

        try
        {
            var strategy = new JsonLoggerStrategy();
            strategy.LocalWrite(new { Key = "value" }, logPath);

            var content = File.ReadAllText(logPath + ".json");
            Assert.Contains("\"Key\"", content);
            Assert.Contains("\"value\"", content);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LocalWrite_AppendsMultipleEntries()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var logPath = Path.Combine(root, "log");

        try
        {
            var strategy = new JsonLoggerStrategy();
            strategy.LocalWrite(new { Seq = 1 }, logPath);
            strategy.LocalWrite(new { Seq = 2 }, logPath);

            var content = File.ReadAllText(logPath + ".json");
            // Both entries should be in the file.
            Assert.Contains("\"Seq\": 1", content);
            Assert.Contains("\"Seq\": 2", content);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Lock property is non-null ─────────────────────────────────────────────

    [Fact]
    public void Lock_IsNotNull()
    {
        var strategy = new JsonLoggerStrategy();

        Assert.NotNull(strategy.Lock);
    }
}
