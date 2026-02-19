namespace EasyLog;

public interface ILoggerStrategy
{
    /// <summary>
    /// Write a log entry to the local file system.
    /// </summary>
    void LocalWrite<T>(T logEntry, string fullPath);

    /// <summary>
    /// Wraps the serialized entry in a <see cref="RemoteLogEntry"/> envelope and sends it as format.
    /// </summary>
    void RemoteWrite<T>(T logEntry, string host, int port);
}