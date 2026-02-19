namespace EasyLog;

public interface ILoggerStrategy
{
    /// <summary>
    /// Write a log entry to the specified path.
    /// </summary>
    /// <param name="logEntry">Log content</param>
    /// <param name="fullPath">Log file path</param>
    /// <typeparam name="T">Object to log</typeparam>
    void LocalWrite<T>(T logEntry, string fullPath);
    
    void RemoteWrite<T>(T logEntry);
}