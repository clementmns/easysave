using System.Text.Json;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// JSON logging strategy implementation
/// </summary>
public class JsonLoggerStrategy : ILoggerStrategy
{
    /// <summary>
    /// Cached JSON serializer options for performance
    /// </summary>
    private static readonly JsonSerializerOptions CachedOptions = new ()
    {
        WriteIndented = true
    };
        
    public void LocalWrite<T>(T logEntry, string logFilePath)
    {
        var jsonString = JsonSerializer.Serialize(logEntry, CachedOptions);

        try
        {
            File.AppendAllText(logFilePath + ".json", jsonString + "," + Environment.NewLine);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EasyLog] Error writing JSON log: {ex.Message}");
        }
    }
        
    public void RemoteWrite<T>(T logEntry) { }
}