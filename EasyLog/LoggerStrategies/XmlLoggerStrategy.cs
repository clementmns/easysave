using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// XML logging strategy implementation.
/// </summary>
public class XmlLoggerStrategy : ILoggerStrategy
{
    public string Extension => "xml";

    private static readonly XmlWriterSettings CachedOptions = new()
    {
        Indent = true,
        Encoding = Encoding.UTF8,
        OmitXmlDeclaration = true
    };

    public void LocalWrite<T>(T logEntry, string logFilePath)
    {
        try
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stream = new FileStream(logFilePath + ".xml", FileMode.Append, FileAccess.Write);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            using var xmlWriter = XmlWriter.Create(writer, CachedOptions);
            xmlSerializer.Serialize(xmlWriter, logEntry);
            writer.WriteLine();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EasyLog] Error writing XML log: {ex.Message}");
        }
    }

    /// <summary>
    /// Protocol: "xml\n{payload}"
    /// </summary>
    public void RemoteWrite<T>(T logEntry, string host, int port)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var memStream = new MemoryStream();
        using (var writer = new StreamWriter(memStream, Encoding.UTF8, leaveOpen: true))
        using (var xmlWriter = XmlWriter.Create(writer, CachedOptions))
            xmlSerializer.Serialize(xmlWriter, logEntry);

        var payload = Extension + "\n" + Encoding.UTF8.GetString(memStream.ToArray());

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, port);
        socket.Send(Encoding.UTF8.GetBytes(payload));
        socket.Shutdown(SocketShutdown.Both);
    }
}
