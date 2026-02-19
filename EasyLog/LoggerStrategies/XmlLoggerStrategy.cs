using System.Xml;
using System.Xml.Serialization;

namespace EasyLog.LoggerStrategies;

/// <summary>
/// XML logging strategy implementation
/// </summary>
public class XmlLoggerStrategy : ILoggerStrategy
{
    private static readonly XmlWriterSettings CachedOptions = new()
    {
        Indent = true,
        Encoding = System.Text.Encoding.UTF8,
        OmitXmlDeclaration = true
    };
        
    public void LocalWrite<T>(T logEntry, string logFilePath)
    {
        try
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var xmlFilePath = logFilePath + ".xml";
            using var stream = new FileStream(xmlFilePath, FileMode.Append, FileAccess.Write);
            using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
            using var xmlWriter = XmlWriter.Create(writer, CachedOptions);
            xmlSerializer.Serialize(xmlWriter, logEntry);
            writer.WriteLine(); // Ensure each entry is on a new line
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EasyLog] Error writing XML log: {ex.Message}");
        }
    }
    
    public void RemoteWrite<T>(T logEntry) { }
}