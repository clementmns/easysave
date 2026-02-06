namespace EasySave.Model;

/// <summary>
/// Application settings.
/// </summary>
public class Settings
{
    /// <summary>
    /// The application version for which these settings are valid.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    public string Language { get; set; } = "en-US";
}
