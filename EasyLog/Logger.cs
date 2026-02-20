using System.Net.Sockets;

namespace EasyLog;

/// <summary>
/// Singleton Logger class for logging messages using different strategies.
/// </summary>
public class Logger : IDisposable
{
    private static Logger? _instance;
    private List<ILoggerStrategy> _strategies = [];

    private LogMode _logMode = LogMode.Local;
    private string? _logFilePath;

    private Socket? _socket;

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

        _instance._socket = logMode switch
        {
            LogMode.Remote or LogMode.Both when (remoteHost is null || remotePort is null) => throw
                new ArgumentException("Remote host and port must be provided for Remote or Both log modes."),
            LogMode.Remote or LogMode.Both => InitRemoteConnection(remoteHost, remotePort.Value),
            _ => _instance._socket
        };

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

        var filePath = _logMode is LogMode.Local or LogMode.Both 
            ? Path.Combine(_logFilePath!, DateTime.Now.ToString("yyyy-MM-dd")) 
            : null;

        foreach (var strategy in _strategies)
        {
            if (filePath is not null)
                strategy.LocalWrite(logEntry, filePath);

            if (_logMode is LogMode.Remote or LogMode.Both)
                strategy.RemoteWrite(logEntry, GetSocket());
        }
    }

    private Socket GetSocket()
    {
        return _socket ?? throw new InvalidOperationException("Socket not initialized. Call Logger.Init() with remote configuration first.");
    }

    private static Socket InitRemoteConnection(string host, int port)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true;
        socket.Connect(host, port);
        return socket;
    }

    private static void CloseRemoteConnection(Socket? socket = null) =>socket?.Shutdown(SocketShutdown.Both);
    
    public void Dispose() => CloseRemoteConnection(_socket);
}