using System.Diagnostics;

namespace EasyLog;

/// <summary>
/// Singleton Logger class for logging messages using different strategies.
/// </summary>
public class Logger
{
    private static Logger? _instance;

    private List<ILoggerStrategy> _strategies = [];

    private LogMode _logMode = LogMode.Local;

    private string? _logFilePath;

    private string? _remoteHost;
    private int? _remotePort;

    /// <summary>
    /// Get the singleton instance of the logger.
    /// </summary>
    /// <exception cref="InvalidOperationException">You need to initialize the logger in the main program first.</exception>
    public static Logger Instance => _instance ?? throw new InvalidOperationException("Logger not initialized. Call Logger.Init() first.");

    /// <summary>
    /// Initialize the logger with application save directory, strategies, mode and optional remote server config.
    /// </summary>
    /// <param name="appSaveDirectory">Directory where local log files are stored.</param>
    /// <param name="strategies">List of logging strategies.</param>
    /// <param name="logMode">Log mode (Local, Remote, or Both).</param>
    /// <param name="remoteHost">Remote server hostname or IP (used for Remote/Both modes).</param>
    /// <param name="remotePort">Remote server port (used for Remote/Both modes).</param>
    public static void Init(
        string appSaveDirectory,
        List<ILoggerStrategy> strategies,
        LogMode? logMode, 
        string? remoteHost,
        int? remotePort)
    {
        _instance ??= new Logger();
        _instance._strategies = strategies;
        _instance._logMode = logMode ?? LogMode.Local;
        
        if (logMode is LogMode.Remote or LogMode.Both && (remoteHost is null || remotePort is null))
            throw new ArgumentException("Remote host and port must be provided for Remote or Both log modes.");

        if (logMode is LogMode.Remote or LogMode.Both)
        {
            _instance._remoteHost = remoteHost;
            _instance._remotePort = remotePort;
        }

        if (_instance._logMode is not (LogMode.Local or LogMode.Both)) return;
        
        _instance._logFilePath = Path.Combine(appSaveDirectory, "Logs");
        if (!Directory.Exists(_instance._logFilePath))
            Directory.CreateDirectory(_instance._logFilePath);
    }

    /// <summary>
    /// Write a log entry using all registered strategies according to the current LogMode.
    /// </summary>
    /// <param name="logEntry">Log content.</param>
    /// <typeparam name="T">Type of the object to log.</typeparam>
    public void Write<T>(T logEntry)
    {
        if (_strategies.Count == 0) return;
        foreach (var strategy in _strategies)
        {
            if (_logMode is LogMode.Local or LogMode.Both)
            {
                var fileName = $"{DateTime.Now:yyyy-MM-dd}";
                var fullPath = Path.Combine(_logFilePath!, fileName);
                strategy.LocalWrite(logEntry, fullPath);
            }
            else {
                if (_remoteHost is null || _remotePort is null) return;
                try
                {
                    strategy.RemoteWrite(logEntry, _remoteHost, _remotePort.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EasyLog] Remote write failed: {ex.Message}");
                }
            }
        }
    }
}
