using System.Text.Json.Serialization;

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
    
    public List<string> CryptExtensions { get; set; } = [];
    
    /// <summary>
    /// Extensions that should be processed with priority during backups
    /// </summary>
    public List<string> PriorityExtensions { get; set; } = [];
    
    /// <summary>
    /// Name of the business software process to monitor (e.g., "CalculatorApp" for Windows calculator)
    /// </summary>
    public string BusinessSoftwareProcessName { get; set; } = "CalculatorApp";
    
    [JsonIgnore]
    public int MaxJobs { get; set; }
    
    [JsonIgnore]
    public string AppSaveDirectory { get; set; } = "";
}
