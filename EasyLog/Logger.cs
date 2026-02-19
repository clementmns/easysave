namespace EasyLog;

/// <summary>
/// Singleton Logger class for logging messages using different strategies
/// </summary>
public class Logger
{
    private static Logger? _instance;
    
    private List<ILoggerStrategy> _strategies = [];
    
    private LogMode _logMode = LogMode.Local;
    
    private string? _logFilePath;

    /// <summary>
    /// Get the singleton instance of the logger
    /// </summary>
    /// <exception cref="InvalidOperationException">You need to initialize the logger in the main program first</exception>
    public static Logger Instance => _instance ?? throw new InvalidOperationException("Logger not initialized. Call Logger.Init() first.");

    /// <summary>
    /// Initialize the logger with application name and logging strategies
    /// </summary>
    /// <param name="appSaveDirectory">Name of the application</param>
    /// <param name="strategies">List of logging strategies</param>
    /// <param name="logMode">Log mode</param>
    public static void Init(string appSaveDirectory, List<ILoggerStrategy> strategies, LogMode? logMode)
    {
        _instance ??= new Logger();
        _instance._strategies = strategies;
        _instance._logMode = logMode ?? LogMode.Local;

        switch (_instance._logMode)
        {
            case LogMode.Both :
            case LogMode.Local:
            default:
                _instance._logFilePath = Path.Combine(appSaveDirectory, "Logs");
                if (!Directory.Exists(_instance._logFilePath)) Directory.CreateDirectory(_instance._logFilePath);
                break;
            case LogMode.Remote:
                // Create socket connection with remote server
                break;
        }
    }

    /// <summary>
    /// Modify the logging strategies.
    /// </summary>
    /// <param name="strategies">List of logging strategies</param>
    /// <exception cref="InvalidOperationException">Logger must be initialized first</exception>
    public static void ModifyStrategies(List<ILoggerStrategy> strategies)
    {
        if (_instance == null) throw new InvalidOperationException("Logger not initialized. Call Logger.Init() first.");
        _instance._strategies = strategies;
    }
    
    /// <summary>
    /// Write a log entry to the log file
    /// </summary>
    /// <param name="logEntry">Log content</param>
    /// <typeparam name="T">Object to log</typeparam>
    public void Write<T>(T logEntry)
    {
        if (_strategies.Count == 0) return;

        switch (_logMode)
        {
            case LogMode.Local:
            case LogMode.Both:
                var fileName = $"{DateTime.Now:yyyy-MM-dd}";
                var fullPath = Path.Combine(_logFilePath, fileName);
        
                foreach (var strategy in _strategies) strategy.LocalWrite(logEntry, fullPath);
                break;
            case LogMode.Remote: break;
        }
        
        
    }
}