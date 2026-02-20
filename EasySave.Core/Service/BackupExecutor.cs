using EasySave.Core.Model;
using EasySave.Core.Model.BackupStrategies;
using EasyLog;
using EasySave.Core.Resources;

namespace EasySave.Core.Service;

public class BackupExecutor
{
    private static int _activePriorityJobs;

    public async Task<bool> ExecuteJobAsync(BackupJob job)
    {
        // Check if business software is running
        if (ProcessMonitorService.IsBusinessSoftwareRunning)
        {
            Logger.Instance.Write(new LogEntry(Errors.BackupBlocked, job, isError: true));
            return false;
        }

        var settings = SettingsService.GetInstance.Settings;
        var priorityExtensions = settings.PriorityExtensions ?? new List<string>();
        var hasPriority = HasPriorityFiles(job, priorityExtensions);

        if (hasPriority)
        {
            Interlocked.Increment(ref _activePriorityJobs);
        }
        else
        {
            // Wait for the active priority jobs to finish before starting a non-priority job
            while (Interlocked.CompareExchange(ref _activePriorityJobs, 0, 0) > 0)
            {
                await Task.Delay(100);
            }
        }

        try
        {
            var strategy = GetStrategy(job);
            await strategy.ExecuteAsync(job, priorityExtensions);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Instance.Write(new LogEntry($"Error during backup: {ex.Message}", job, isError: true));
            return false;
        }
        finally
        {
            if (hasPriority)
                Interlocked.Decrement(ref _activePriorityJobs);
        }
    }

    private static bool HasPriorityFiles(BackupJob job, List<string> priorityExtensions)
    {
        if (priorityExtensions.Count == 0)
            return false;

        try
        {
            if (File.Exists(job.SourcePath))
            {
                var ext = Path.GetExtension(job.SourcePath);
                return priorityExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
            }

            if (Directory.Exists(job.SourcePath))
            {
                return Directory.EnumerateFiles(job.SourcePath, "*.*", SearchOption.AllDirectories)
                    .Any(f => priorityExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static IBackupStrategy GetStrategy(BackupJob job)
    {
        return job.Type switch
        {
            BackupType.Full => new FullBackupStrategy(),
            BackupType.Differential => new DifferentialBackupStrategy(),
            _ => throw new InvalidOperationException("Backup type not supported")
        };
    }
}
