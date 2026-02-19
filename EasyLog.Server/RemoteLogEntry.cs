namespace EasyLog.Server;

/// <summary>
/// Mirror of EasyLog.RemoteLogEntry — kept local to avoid a project reference.
/// Must stay in sync with the client-side class.
/// </summary>
public sealed class RemoteLogEntry
{
    public string Format { get; init; } = "json"; 
    public string Content { get; init; } = string.Empty;
}