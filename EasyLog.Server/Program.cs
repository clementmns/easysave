using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyLog.Server;

internal static class Program
{
    private const int Port = 5092;
    private const string LogDirectory = "logs";

    private static readonly object JsonLock = new();
    private static readonly object XmlLock  = new();

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

            var message = Encoding.UTF8.GetString(memStream.ToArray());
            if (string.IsNullOrWhiteSpace(message)) return;

            // First line = extension, rest = payload
            var newlineIndex = message.IndexOf('\n');
            if (newlineIndex < 0) return;

            var extension = message[..newlineIndex].Trim();
            var content   = message[(newlineIndex + 1)..];

            AppendToLog(content, extension);

            Console.WriteLine($"[INFO] - Wrote {content.Length} bytes (format={extension}) from {endpoint}");
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

    private static void AppendToLog(string content, string extension)
    {
        var fileLock = extension == "xml" ? XmlLock : JsonLock;
        var filePath = Path.Combine(LogDirectory, DateTime.Now.ToString("yyyy-MM-dd") + "." + extension);

        lock (fileLock) File.AppendAllText(filePath, content + Environment.NewLine, Encoding.UTF8);
    }
}
