using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.Server;

internal static class Program
{
    private const int Port = 5092;
    private const string LogDirectory = "logs";

    private static readonly Lock DefaultLock = new();
    private static readonly Dictionary<string, object> FileLocks = new();

    static async Task Main(string[] args)
    {
        Directory.CreateDirectory(LogDirectory);

        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 0));
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
        serverSocket.Listen(128);

        Console.WriteLine($"[INFO] - Listening on port {Port}");

        while (true)
        {
            var client = await Task.Run(() => serverSocket.Accept());
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static void HandleClient(Socket client)
    {
        var endpoint = client.RemoteEndPoint?.ToString() ?? "unknown";
        Console.WriteLine($"[INFO] - Client connected: {endpoint}");

        try
        {
            // Read until EOF
            using var memStream = new MemoryStream();
            var buffer = new byte[4096];
            int n;
            while ((n = client.Receive(buffer)) > 0)
                memStream.Write(buffer, 0, n);

            var raw = Encoding.UTF8.GetString(memStream.ToArray());
            if (string.IsNullOrWhiteSpace(raw)) return;

            var entry = JsonSerializer.Deserialize<RemoteLogEntry>(raw);
            if (entry is null) return;

            AppendToLog(entry.Content, entry.Format);

            Console.WriteLine($"[INFO] - Wrote {entry.Content.Length} bytes (format={entry.Format}) from {endpoint}");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"[ERROR] - Socket error from {endpoint}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] - Error handling client {endpoint}: {ex.Message}");
        }
        finally
        {
            try { client.Shutdown(SocketShutdown.Both); } catch { /* ignore */ }
            client.Close();
            Console.WriteLine($"[INFO] - Client disconnected: {endpoint}");
        }
    }

    private static void AppendToLog(string content, string format)
    {
        var fileLock = GetFileLock(format);
        var filePath = Path.Combine(LogDirectory, DateTime.Now.ToString("yyyy-MM-dd") + "." + format);

        lock (fileLock) File.AppendAllText(filePath, content + Environment.NewLine, Encoding.UTF8);
    }

    /// <summary>
    /// Returns a per-format lock, creating one on first use.
    /// Any new format (csv, YAML, ...) is handled automatically.
    /// </summary>
    private static object GetFileLock(string format)
    {
        lock (DefaultLock)
        {
            if (!FileLocks.TryGetValue(format, out var fileLock))
                FileLocks[format] = fileLock = new object();
            return fileLock;
        }
    }
}
