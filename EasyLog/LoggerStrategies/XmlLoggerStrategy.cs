using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// XML logging strategy implementation.
/// </summary>
public class XmlLoggerStrategy : ILoggerStrategy
{
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

    public void RemoteWrite<T>(T logEntry, Socket socket)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var memStream = new MemoryStream();
        using (var writer = new StreamWriter(memStream, Encoding.UTF8, leaveOpen: true))
        using (var xmlWriter = XmlWriter.Create(writer, CachedOptions))
            xmlSerializer.Serialize(xmlWriter, logEntry);

        var envelope = new RemoteLogEntry
        {
            Format  = "xml",
            Content = Encoding.UTF8.GetString(memStream.ToArray())
        };
        
        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        socket.Send(message);
    }
}
