namespace EasySave.Core.Service;

public class TransferLimitService
{
    private static readonly Lock Lock = new();
    
    private readonly SemaphoreSlim _largeFileSemaphore = new(1, 1);
    private SemaphoreSlim? _smallFileSemaphore;

    private int _pendingPriorityFiles;
    private readonly object _priorityLock = new();
    
    private bool _initialized;
    
    public static TransferLimitService Instance
    {
        get
        {
            var instance = field;
            if (instance != null) return instance;
            lock (Lock)
            {
                return field ??= new TransferLimitService();
            }
        }
    }

    public void Initialize()
    {
        if (_initialized) return;

        var maxParallelJobs = SettingsService.GetInstance.Settings.MaxJobs;
        
        _smallFileSemaphore = new SemaphoreSlim(maxParallelJobs > 0 ? maxParallelJobs : 3, maxParallelJobs > 0 ? maxParallelJobs : 3);
        _initialized = true;
    }

    public void WaitForFileTransfer(long fileSizeInBytes)
    {
        var sizeThresholdBytes = SettingsService.GetInstance.Settings.MaxTransferSizeForParallel * 1024;
        var isLargeFile = fileSizeInBytes > sizeThresholdBytes;
        
        var semaphore = isLargeFile ? _largeFileSemaphore : _smallFileSemaphore;
        if (semaphore == null) throw new InvalidOperationException("TransferLimitService not initialized");
        semaphore.Wait();
    }

    public void ReleaseFileTransfer(long fileSizeInBytes)
    {
        var sizeThresholdBytes = SettingsService.GetInstance.Settings.MaxTransferSizeForParallel * 1024;
        var isLargeFile = fileSizeInBytes > sizeThresholdBytes;
        
        var semaphore = isLargeFile ? _largeFileSemaphore : _smallFileSemaphore;
        if (semaphore == null) throw new InvalidOperationException("TransferLimitService not initialized");
        semaphore.Release();
    }

    public void WaitForPriorityFile(bool isPriorityFile)
    {
        if (isPriorityFile) return;
        lock (_priorityLock)
        {
            while (_pendingPriorityFiles > 0)
            {
                Monitor.Wait(_priorityLock);
            }
        }
    }

    public void AddPendingPriorityFile()
    {
        lock (_priorityLock)
        {
            _pendingPriorityFiles++;
        }
    }

    public void RemovePendingPriorityFile()
    {
        lock (_priorityLock)
        {
            _pendingPriorityFiles--;
            if (_pendingPriorityFiles == 0)
            {
                Monitor.PulseAll(_priorityLock);
            }
        }
    }
}
