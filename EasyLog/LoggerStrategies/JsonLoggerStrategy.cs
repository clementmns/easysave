using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// JSON logging strategy implementation.
/// </summary>
public class JsonLoggerStrategy : ILoggerStrategy
{
    public string Extension => "json";

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

    /// <summary>
    /// Protocol: "json\n{payload}"
    /// </summary>
    public void RemoteWrite<T>(T logEntry, string host, int port)
    {
        var message = Extension + "\n" + JsonSerializer.Serialize(logEntry, CachedOptions);

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, port);
        socket.Send(Encoding.UTF8.GetBytes(message));
        socket.Shutdown(SocketShutdown.Both);
    }
}
