using EasySave.Core.Resources;
using EasySave.Core.Utils;
using EasySave.Core.Service;
using EasyLog;

namespace EasySave.Core.Model.BackupStrategies;

/// <summary>
/// Strategy for backing up a directory or a file recursively using full copy.
/// </summary>
public class FullBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        var cryptedExtensions = SettingsService.GetInstance.Settings.CryptExtensions;
        var priorityExtensions = SettingsService.GetInstance.Settings.PriorityExtensions;
        
        var result = (File.Exists(job.SourcePath), Directory.Exists(job.SourcePath)) switch
        {
            (true, false) => ProcessFile(job, cryptedExtensions, priorityExtensions),
            (false, true) => ProcessDirectory(job, cryptedExtensions, priorityExtensions),
            _ => throw new FileNotFoundException(Errors.ProcessingError)
        };

        return result;
    }

    private static void CopyOrEncryptFile(
        string sourceFile,
        string destFile,
        string sourceRoot,
        string destFolder,
        List<string> cryptExt,
        BackupJob job,
        bool isPriority = false,
        Action<long, long>? onProgress = null)
    {
        var fileInfo = new FileInfo(sourceFile);
        var dirName = Path.GetDirectoryName(destFile);

        if (!string.IsNullOrEmpty(dirName))
            Directory.CreateDirectory(dirName);

        TransferLimitService.Instance.WaitForPriorityFile(isPriority);
        if (isPriority) TransferLimitService.Instance.AddPendingPriorityFile();
        TransferLimitService.Instance.WaitForFileTransfer(fileInfo.Length);
        
        try
        {
            if (cryptExt.Contains(fileInfo.Extension))
            {
                var resultEncryption = CryptoUtils.EncryptFile(sourceFile, destFile);
                if (!resultEncryption.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destFile)}", job, true));
                    throw new Exception(Errors.FileCantBeCrypted);
                }
                Logger.Instance.Write(new LogEntry($"File Encrypted : {destFolder}", job, false, null, resultEncryption.Item2));
            }
            else
            {
                var resultCopy = FileUtils.CopyFile(sourceFile, destFolder, sourceRoot, onProgress);
                if (!resultCopy.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Copy failed : {destFolder}", job, true));
                    throw new Exception(Errors.FileCantBeCopied);
                }
                Logger.Instance.Write(new LogEntry($"File Copied : {destFolder}", job, false, resultCopy.Item2, null));
            }
        }
        finally
        {
            TransferLimitService.Instance.ReleaseFileTransfer(fileInfo.Length);
            if (isPriority) TransferLimitService.Instance.RemovePendingPriorityFile();
        }
    }

    private static bool ProcessFile(BackupJob job, List<string> cryptExt, List<string> priorityExtensions)
    {
        try
        {
            var fileInfo = new FileInfo(job.SourcePath);
            
            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = fileInfo.Length;
            job.State.RemainingFilesSize = fileInfo.Length;
            job.State.Progression = 0;
            job.State.CurrentFileName = fileInfo.Name;
            job.State.CurrentFileSize = fileInfo.Length;
            
            var sourcePath = fileInfo.FullName;
            var sourceRoot = Path.GetDirectoryName(sourcePath);
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot) ? fileInfo.Name : Path.GetRelativePath(sourceRoot, sourcePath);
            var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

            var extension = fileInfo.Extension;
            var isPriority = priorityExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

            CopyOrEncryptFile(sourcePath, destinationFilePath, sourceRoot ?? string.Empty, job.DestinationPath, cryptExt, job, isPriority, OnProgress);

            job.State.RemainingFiles = 0;
            job.State.RemainingFilesSize = 0;
            job.State.Progression = 100;
            return true;

            void OnProgress(long bytesTransferred, long totalBytes)
            {
                job.State.RemainingFilesSize = totalBytes - bytesTransferred;
                job.State.Progression = (int)(100.0 * bytesTransferred / totalBytes);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool ProcessDirectory(BackupJob job, List<string> cryptExt, List<string> priorityExtensions)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(job.SourcePath);
            var destinationBackupFolder = Path.Combine(job.DestinationPath, Path.GetFileName(job.SourcePath));
            
            Directory.CreateDirectory(destinationBackupFolder);
            
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();

            var (priorityFiles, nonPriorityFiles) = FileUtils.SeparatePriorityFiles(files, priorityExtensions);
            
            var orderedFiles = priorityFiles.Concat(nonPriorityFiles).ToList();

            job.State.TotalFiles = orderedFiles.Count;
            job.State.FileSize = orderedFiles.Sum(f => f.Length);
            job.State.RemainingFiles = orderedFiles.Count;
            job.State.RemainingFilesSize = job.State.FileSize;
            job.State.Progression = 0;
            job.State.CurrentFileName = string.Empty;
            job.State.CurrentFileSize = 0;

            foreach (var file in orderedFiles)
            {
                job.PauseGate.Wait(job.CancellationTokenSource.Token);
                job.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                job.State.CurrentFileName = file.Name;
                job.State.CurrentFileSize = file.Length;

                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);

                var isPriority = priorityFiles.Contains(file);

                job.State.RemainingFilesSize -= file.Length;
                
                CopyOrEncryptFile(file.FullName, destinationFilePath, job.SourcePath, destinationBackupFolder, cryptExt, job, isPriority, OnFileProgress);
                
                job.State.RemainingFiles -= 1;
                job.State.Progression = (int)(100.0 * (1.0 - ((double)job.State.RemainingFilesSize / job.State.FileSize)));

                void OnFileProgress(long bytesTransferred, long totalBytes)
                {
                    var bytesRemainingInCurrentFile = totalBytes - bytesTransferred;
                    var totalBytesRemaining = job.State.RemainingFilesSize + bytesRemainingInCurrentFile;
                    var overallProgress = 1.0 - ((double)totalBytesRemaining / job.State.FileSize);
                    job.State.Progression = (int)(100.0 * overallProgress);
                }
            }
            
            job.State.Progression = 100;
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }
}