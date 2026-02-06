namespace EasySave.Model;

/// <summary>
/// Log entry for a backup job.
/// </summary>
public class LogEntry
{
    public string Message { get; set; }
    public string BackupName { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourcePath { get; set; }
    public string DestinationPath { get; set; }
    public long FileSize { get; set; }
    public long? TransferDuration { get; set; }
    
    public bool? IsError { get; set; }
    
    private LogEntry(string message, string backupName, string sourcePath, string destinationPath, long fileSize, bool? isError = false, long? transferDuration = null)
    {
        Message = message;
        BackupName = backupName;
        Timestamp = DateTime.Now;
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        FileSize = fileSize;
        IsError = isError;
        TransferDuration = transferDuration;
    }
    
    public LogEntry( string message, BackupJob job, bool? isError = false, long? transferDuration = null) : this(message, job.Name, job.SourcePath, job.DestinationPath, job.State.FileSize, isError, transferDuration) { }
}