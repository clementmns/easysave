using EasyLog.LoggerStrategies;

namespace EasySave.Tests.EasyLog;

public class XmlLoggerStrategyTests
{
    // ── LocalWrite creates file ───────────────────────────────────────────────

    [Fact]
    public void LocalWrite_CreatesXmlFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var logPath = Path.Combine(root, "log");

        try
        {
            var strategy = new XmlLoggerStrategy();
            strategy.LocalWrite(new SimpleEntry { Text = "test" }, logPath);

            Assert.True(File.Exists(logPath + ".xml"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LocalWrite_ContentContainsXmlTags()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var logPath = Path.Combine(root, "log");

        try
        {
            var strategy = new XmlLoggerStrategy();
            strategy.LocalWrite(new SimpleEntry { Text = "xml_value" }, logPath);

            var content = File.ReadAllText(logPath + ".xml");
            Assert.Contains("xml_value", content);
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
            var strategy = new XmlLoggerStrategy();
            strategy.LocalWrite(new SimpleEntry { Text = "first" }, logPath);
            strategy.LocalWrite(new SimpleEntry { Text = "second" }, logPath);

            var content = File.ReadAllText(logPath + ".xml");
            Assert.Contains("first", content);
            Assert.Contains("second", content);
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
        var strategy = new XmlLoggerStrategy();

        Assert.NotNull(strategy.Lock);
    }

    // ── Helper DTO (must be XML-serializable, i.e. public with parameterless ctor) ──

    public class SimpleEntry
    {
        public string? Text { get; set; }
    }
}
