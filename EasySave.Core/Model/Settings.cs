using System.Text.Json.Serialization;
using EasyLog;

namespace EasySave.Core.Model;

/// <summary>
/// Application settings.
/// </summary>
public class Settings
{
    /// <summary>
    /// The application version for which these settings are valid.
    /// </summary>
    public string Version { get; set; } = "1.1.0";

    public string Language { get; set; } = "en-US";
    
    public LogFormat LogFormat { get; set; } = LogFormat.Json;

    /// <summary>
    /// Log destination: Local, Remote, or Both.
    /// </summary>
    public LogMode LogMode { get; set; } = LogMode.Local;

    /// <summary>
    /// Hostname or IP of the EasyLog.Server instance.
    /// </summary>
    public string LogServerHost { get; set; } = "localhost";

    /// <summary>
    /// TCP port of the EasyLog.Server instance.
    /// </summary>
    public int LogServerPort { get; set; } = 5092;

    public List<string> CryptExtensions { get; set; } = [];

    public string? CryptoSoftPath { get; set; }
    
    /// <summary>
    /// Extensions that should be processed with priority during backups
    /// </summary>
    public List<string> PriorityExtensions { get; set; } = [];

    /// <summary>
    /// Name of the business software process to monitor (e.g., "CalculatorApp" for Windows calculator)
    /// </summary>
    public string BusinessSoftwareProcessName { get; set; } = "CalculatorApp";

    /// <summary>
    /// Maximum file size in KB for parallel transfers.
    /// Files larger than this cannot be transferred in parallel with other large files.
    /// </summary>
    public long MaxTransferSizeForParallel { get; set; } = 5096;
    
    [JsonIgnore]
    public int MaxJobs { get; set; } = 10;
    
    [JsonIgnore]
    public string AppSaveDirectory { get; set; } = "";
}
