using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.Server;

internal static class Program
{
    private const int MaxConnections = 10;
    private const int Port = 5092;
    private const string LogDirectory = "logs";

    private static readonly ConcurrentDictionary<string, object> FileLocks = new();
    
    private static void Main(string[] args)
    {
        Directory.CreateDirectory(LogDirectory);

        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
        serverSocket.Listen(128);

        Console.WriteLine($"[INFO] - Listening on port {Port}");

        var semaphore = new SemaphoreSlim(MaxConnections);
        while (true)
        {
            var client = serverSocket.Accept();
            semaphore.Wait();

            Task.Run(() =>
            {
                try
                {
                    HandleClient(client);
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }
    }

    private static void HandleClient(Socket client)
    {
        var endpoint = client.RemoteEndPoint?.ToString() ?? "unknown";
        Console.WriteLine($"[INFO] - Client connected: {endpoint}");

        try
        {
            var pendingData = new List<byte>();
            var buffer = new byte[4096];

            int bytesRead;
            while ((bytesRead = client.Receive(buffer)) > 0)
            {
                pendingData.AddRange(buffer.Take(bytesRead));
                ProcessPendingData(pendingData, endpoint);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[ERROR] - Socket error from {endpoint}: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine($"[INFO] - Client disconnected: {endpoint}");
        }
    }

    private static void ProcessPendingData(List<byte> pendingData, string endpoint)
    {
        while (pendingData.Count > 0)
        {
            var reader = new Utf8JsonReader(pendingData.ToArray(), isFinalBlock: false, state: default);

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                return;

            try
            {
                var entry = JsonSerializer.Deserialize<RemoteLogEntry>(ref reader);
                if (entry is null) return;

                AppendToLog(entry.Content, entry.Format);
                Console.WriteLine($"[INFO] - Wrote {entry.Content.Length} bytes (format={entry.Format}) from {endpoint}");
                pendingData.RemoveRange(0, (int)reader.BytesConsumed);
            }
            catch (JsonException)
            {
                return;
            }
        }
    }

    private static void AppendToLog(string content, string format)
    {
        var fileLock = GetFileLock(format);
        var filePath = Path.Combine(LogDirectory, DateTime.Now.ToString("yyyy-MM-dd") + "." + format);

        lock (fileLock) File.AppendAllText(filePath, content + Environment.NewLine, Encoding.UTF8);
    }

    private static object GetFileLock(string format) => 
        FileLocks.GetOrAdd(format, _ => new object());
}
