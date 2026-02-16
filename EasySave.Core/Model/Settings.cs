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
    
    /// <summary>
    /// Name of the business software process to monitor (e.g., "CalculatorApp" for Windows calculator)
    /// </summary>
    public string BusinessSoftwareProcessName { get; set; } = "CalculatorApp";
    
    [JsonIgnore]
    public int MaxJobs { get; set; }
    
    [JsonIgnore]
    public string AppSaveDirectory { get; set; } = "";
}
