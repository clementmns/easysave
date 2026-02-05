namespace EasySave.ConsoleApp.Model;

public class LogEntry
{
    public string Message { get; set; }
    public string BackupName { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourcePath { get; set; }
    public string DestinationPath { get; set; }
    public long FileSize { get; set; }
    public int TransferDuration { get; set; }
    
    public LogEntry(string message, string backupName, string sourcePath, string destinationPath, long fileSize, int transferDuration)
    {
        Message = message;
        BackupName = backupName;
        Timestamp = DateTime.Now;
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        FileSize = fileSize;
        TransferDuration = transferDuration;
    }
}