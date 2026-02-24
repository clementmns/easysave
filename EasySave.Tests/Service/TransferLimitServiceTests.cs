using System.Diagnostics;
using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class TransferLimitServiceTests
{
    public TransferLimitServiceTests()
    {
        try
        {
            _ = SettingsService.GetInstance;
        }
        catch
        {
            SettingsService.Init(new TestAppProperties());
        }
    }

    [Fact]
    public void Initialize_SetsUpSemaphores()
    {
        var service = new TransferLimitService();
        service.Initialize();

        var maxJobs = SettingsService.GetInstance.Settings.MaxJobs;
        
        for (int i = 0; i < maxJobs; i++)
        {
            service.WaitForFileTransfer(1000);
        }
        
        var waitTask = Task.Run(() => service.WaitForFileTransfer(1000));
        Thread.Sleep(100);
        
        Assert.False(waitTask.IsCompleted);
        
        service.ReleaseFileTransfer(1000);
        waitTask.Wait();
        
        for (int i = 0; i < maxJobs; i++)
        {
            service.ReleaseFileTransfer(1000);
        }
        
        Assert.True(waitTask.IsCompleted);
    }

    [Fact]
    public void WaitForFileTransfer_AllowsSmallFiles()
    {
        var service = new TransferLimitService();
        service.Initialize();

        service.WaitForFileTransfer(1000);
        service.ReleaseFileTransfer(1000);
    }

    [Fact]
    public void WaitForFileTransfer_AllowsLargeFiles()
    {
        var service = new TransferLimitService();
        service.Initialize();

        var threshold = SettingsService.GetInstance.Settings.MaxTransferSizeForParallel * 1024;
        service.WaitForFileTransfer(threshold + 1);
        service.ReleaseFileTransfer(threshold + 1);
    }

    [Fact]
    public async Task WaitForPriorityFile_BlocksWhenPriorityFilesPending()
    {
        var service = new TransferLimitService();
        service.Initialize();
        
        service.AddPendingPriorityFile();
        
        var waitTask = Task.Run(() => service.WaitForPriorityFile(false));
        await Task.Delay(100);
        
        Assert.False(waitTask.IsCompleted);
        
        service.RemovePendingPriorityFile();
        await waitTask;
        
        Assert.True(waitTask.IsCompleted);
    }

    [Fact]
    public void WaitForPriorityFile_DoesNotBlockPriorityFiles()
    {
        var service = new TransferLimitService();
        service.Initialize();
        
        service.AddPendingPriorityFile();
        
        service.WaitForPriorityFile(true);
        
        service.RemovePendingPriorityFile();
    }

    [Fact]
    public async Task MultipleSmallFiles_CanRunInParallel()
    {
        var service = new TransferLimitService();
        service.Initialize();
        
        var maxJobs = SettingsService.GetInstance.Settings.MaxJobs;
        var tasks = new List<Task>();
        var running = 0;
        var maxRunning = 0;
        var lockObj = new object();

        for (int i = 0; i < maxJobs + 1; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                service.WaitForFileTransfer(1000);
                lock (lockObj)
                {
                    running++;
                    maxRunning = Math.Max(maxRunning, running);
                }
                Thread.Sleep(50);
                lock (lockObj)
                {
                    running--;
                }
                service.ReleaseFileTransfer(1000);
            }));
        }

        await Task.WhenAll(tasks);
        
        Assert.True(maxRunning <= maxJobs);
    }

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = TransferLimitService.Instance;
        var instance2 = TransferLimitService.Instance;
        
        Assert.Same(instance1, instance2);
    }

    private class TestAppProperties : IAppProperties
    {
        public string AppSaveDirectory { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        public int MaxJobs { get; } = 3;
        public string? CryptoSoftPath { get; } = null;
    }
}
