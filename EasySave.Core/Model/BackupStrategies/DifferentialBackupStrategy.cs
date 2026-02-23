using EasySave.Core.Resources;
using EasySave.Core.Utils;
using EasySave.Core.Service;
using EasyLog;

namespace EasySave.Core.Model.BackupStrategies;

/// <summary>
/// Strategy for backing up a directory or a file recursively using differential copy.
/// </summary>
public class DifferentialBackupStrategy : IBackupStrategy
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

    private static void CopyOrEncryptFile(string sourceFile, string destFile, string sourceRoot, string destFolder, List<string> cryptExt, BackupJob job, bool isPriority = false)
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
                var resultCopy = FileUtils.CopyFile(sourceFile, destFolder, sourceRoot);
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
            Console.WriteLine($"File transfered : {destFile}");
            TransferLimitService.Instance.ReleaseFileTransfer(fileInfo.Length);
            if (isPriority) TransferLimitService.Instance.RemovePendingPriorityFile();
        }
    }

    private static bool ProcessFile(BackupJob job, List<string> cryptExt, List<string> priorityExtensions)
    {
        try
        {
            var fileInfo = new FileInfo(job.SourcePath);
        
            var sourcePath = fileInfo.FullName;
            var sourceRoot = Path.GetDirectoryName(sourcePath);
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot) ? fileInfo.Name : Path.GetRelativePath(sourceRoot, sourcePath);
            var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

            var shouldCopy = !File.Exists(destinationFilePath) || fileInfo.LastWriteTime > File.GetLastWriteTime(destinationFilePath);
            if (!shouldCopy)
            {
                job.State.Progression = 100;
                return true;
            }

            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = fileInfo.Length;
            job.State.RemainingFilesSize = fileInfo.Length;
            job.State.Progression = 0;

            var extension = fileInfo.Extension;
            var isPriority = priorityExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            
            CopyOrEncryptFile(sourcePath, destinationFilePath, sourceRoot ?? string.Empty, job.DestinationPath, cryptExt, job, isPriority);
        
            job.State.Progression = 100;
            return true;
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

            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            List<FileInfo> filesToCopy = [];

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);
                if (!File.Exists(destinationFilePath) || file.LastWriteTime > File.GetLastWriteTime(destinationFilePath))
                {
                    filesToCopy.Add(file);
                }
            }

            var (priorityFiles, nonPriorityFiles) = FileUtils.SeparatePriorityFiles(filesToCopy, priorityExtensions);
            var orderedFiles = priorityFiles.Concat(nonPriorityFiles).ToList();

            job.State.TotalFiles = orderedFiles.Count;
            job.State.FileSize = orderedFiles.Sum(f => f.Length);
            job.State.RemainingFiles = orderedFiles.Count;
            job.State.RemainingFilesSize = job.State.FileSize;
            job.State.Progression = 0;

            foreach (var file in orderedFiles)
            {
                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);

                var isPriority = priorityFiles.Contains(file);
                CopyOrEncryptFile(file.FullName, destinationFilePath, job.SourcePath, destinationBackupFolder, cryptExt, job, isPriority);

                job.State.RemainingFiles -= 1;
                job.State.RemainingFilesSize -= file.Length;
                job.State.Progression = (int)(100.0 * (1.0 - ((double)job.State.RemainingFilesSize / job.State.FileSize)));
            }
            
            job.State.Progression = 100;
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}