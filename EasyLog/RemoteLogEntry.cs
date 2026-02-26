namespace EasyLog;

/// <summary>
/// Envelope sent over the TCP socket to the remote log server.
/// The server uses <see cref="Format"/> to pick the right file,
/// and writes <see cref="Content"/> as-is.
/// </summary>
public sealed class RemoteLogEntry
{
    public string Format { get; init; } = "json";
    public string Content { get; init; } = string.Empty;
}
