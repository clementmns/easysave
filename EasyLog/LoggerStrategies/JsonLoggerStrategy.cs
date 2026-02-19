using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// JSON logging strategy implementation.
/// </summary>
public class JsonLoggerStrategy : ILoggerStrategy
{
    private static readonly JsonSerializerOptions CachedOptions = new() { WriteIndented = true };

    public void LocalWrite<T>(T logEntry, string logFilePath)
    {
        try
        {
            File.AppendAllText(logFilePath + ".json",
                JsonSerializer.Serialize(logEntry, CachedOptions) + "," + Environment.NewLine);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EasyLog] Error writing JSON log: {ex.Message}");
        }
    }

    public void RemoteWrite<T>(T logEntry, string host, int port)
    {
        var envelope = new RemoteLogEntry
        {
            Format  = "json",
            Content = JsonSerializer.Serialize(logEntry, CachedOptions)
        };

        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, port);
        socket.Send(message);
        socket.Shutdown(SocketShutdown.Both);
    }
}
