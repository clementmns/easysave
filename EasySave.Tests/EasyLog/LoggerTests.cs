using EasyLog;
using EasyLog.LoggerStrategies;

namespace EasySave.Tests.EasyLog;

public class LoggerTests
{
    // ── Init ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Init_CalledOnce_CreatesInstance()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);

            Assert.NotNull(Logger.Instance);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Init_CreatesLogsSubdirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);

            Assert.True(Directory.Exists(Path.Combine(dir, "Logs")));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Init_CalledTwice_KeepsOriginalInstance()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);
            var first = Logger.Instance;

            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);
            var second = Logger.Instance;

            Assert.Same(first, second);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ── Write with JSON strategy ──────────────────────────────────────────────

    [Fact]
    public void Write_WithJsonStrategy_CreatesLogFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);
            Logger.Instance.Write(new { Message = "test" });

            var logFile = Path.Combine(dir, "Logs", DateTime.Now.ToString("yyyy-MM-dd") + ".json");
            Assert.True(File.Exists(logFile));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Write_WithJsonStrategy_LogFileContainsMessage()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Local, null, null);
            Logger.Instance.Write(new { Message = "unique_marker_xyz" });

            var logFile = Path.Combine(dir, "Logs", DateTime.Now.ToString("yyyy-MM-dd") + ".json");
            var content = File.ReadAllText(logFile);
            Assert.Contains("unique_marker_xyz", content);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ── Write with XML strategy ───────────────────────────────────────────────

    [Fact]
    public void Write_WithXmlStrategy_CreatesLogFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [new XmlLoggerStrategy()], LogMode.Local, null, null);
            Logger.Instance.Write(new SimpleLog { Text = "xml_test" });

            var logFile = Path.Combine(dir, "Logs", DateTime.Now.ToString("yyyy-MM-dd") + ".xml");
            Assert.True(File.Exists(logFile));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ── Write with no strategies ──────────────────────────────────────────────

    [Fact]
    public void Write_WithNoStrategies_DoesNotThrow()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Logger.Init(dir, [], LogMode.Local, null, null);

            // Should silently be a no-op.
            Logger.Instance.Write(new { Message = "ignored" });
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ── Remote mode without host throws ──────────────────────────────────────

    [Fact]
    public void Init_RemoteModeWithoutHost_ThrowsArgumentException()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            Assert.Throws<ArgumentException>(() =>
                Logger.Init(dir, [new JsonLoggerStrategy()], LogMode.Remote, null, null));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ── Helper DTO that is XML-serializable ───────────────────────────────────

    public class SimpleLog
    {
        public string? Text { get; set; }
    }
}
