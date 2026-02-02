namespace EasySave.ConsoleApp.Model;

public class RealTimeState
{
    private string _name { get; set; }
    private DateTime _lastUpdate { get; set; }
    private bool _isActive { get; set; }
    private int _totalFiles { get; set; }
    private long _fileSize { get; set; }
    private int _progression  { get; set; }
    private int _remainingFiles { get; set; }
    private long _remainingFilesSize { get; set; }
    private string _sourceDirectory { get; set; }
    private string _destinationDirectory { get; set; }
    
}