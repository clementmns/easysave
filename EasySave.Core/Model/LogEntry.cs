using static EasySave.Core.Utils.FileUtils;

namespace EasySave.Core.Model;

/// <summary>
/// Log entry for a backup job.
/// </summary>
public class LogEntry
{
    public string? Message { get; set; }
    public string? BackupName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SourcePath { get; set; }
    public string? DestinationPath { get; set; }
    public long FileSize { get; set; }
    public long? CopyDuration { get; set; }
    public long? CryptDuration { get; set; }
    
    public bool? IsError { get; set; }

    public LogEntry() { }
    
    private LogEntry(string message, string backupName, string? sourcePath, string? destinationPath, long fileSize, bool? isError = false, long? copyDuration = null, long? cryptDuration = null)
    {
        Message = message;
        BackupName = backupName;
        Timestamp = DateTime.Now;
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        FileSize = fileSize;
        IsError = isError;
        CopyDuration = copyDuration;
        CryptDuration = cryptDuration;
    }
    
    public LogEntry( string message, BackupJob job, bool? isError = false, long? copyDuration = null, long? cryptDuration = null) : 
        this(
            message, 
            job.Name, 
            ConvertToUnc(job.SourcePath), 
            ConvertToUnc(job.DestinationPath), 
            job.State.FileSize, 
            isError, 
            copyDuration, 
            cryptDuration
        ) { }
}