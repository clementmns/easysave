using System.Net.Sockets;

namespace EasyLog;

public interface ILoggerStrategy
{
    ReaderWriterLockSlim Lock { get; }

    /// <summary>
    /// Write a log entry to the local file system.
    /// </summary>
    void LocalWrite<T>(T logEntry, string fullPath);

    /// <summary>
    /// Wraps the serialized entry in a <see cref="RemoteLogEntry"/> envelope and sends it as format.
    /// </summary>
    void RemoteWrite<T>(T logEntry, Socket socket);
}