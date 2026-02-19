namespace EasyLog;

public interface ILoggerStrategy
{
    /// <summary>
    /// File extension used for log files (e.g. "json", "xml").
    /// Sent as the first line of every remote message so the server knows
    /// which file to write to — no parsing or heuristics needed.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Write a log entry to the local file system.
    /// </summary>
    void LocalWrite<T>(T logEntry, string fullPath);

    /// <summary>
    /// Send a log entry to the remote log server over a raw TCP socket.
    /// Protocol: "{Extension}\n{serialized payload}"
    /// </summary>
    void RemoteWrite<T>(T logEntry, string host, int port);
}