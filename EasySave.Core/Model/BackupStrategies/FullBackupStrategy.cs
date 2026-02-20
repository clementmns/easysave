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
        var result = (File.Exists(job.SourcePath), Directory.Exists(job.SourcePath)) switch
        {
            (true, false) => ProcessFile(job, cryptedExtensions),
            (false, true) => ProcessDirectory(job,cryptedExtensions),
            _ => throw new FileNotFoundException(Errors.ProcessingError)
        };

        return result;
    }

    private static bool ProcessFile(BackupJob job, List<string> cryptExt)
    {
        try
        {
            var fileInfo = new FileInfo(job.SourcePath);
            
            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = fileInfo.Length;
            job.State.RemainingFilesSize = fileInfo.Length;
            job.State.Progression = 0;
            
            if (cryptExt.Contains(fileInfo.Extension))
            {
                var sourcePath = fileInfo.FullName;
                var sourceRoot = Path.GetDirectoryName(sourcePath);
                var relativePath = string.IsNullOrWhiteSpace(sourceRoot) ? fileInfo.Name : Path.GetRelativePath(sourceRoot, sourcePath);
                var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

                var resultEncryption = CryptoUtils.EncryptFile(sourcePath, destinationFilePath);
                if (!resultEncryption.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destinationFilePath)}", job, true));
                    throw new Exception(Errors.FileCantBeCrypted);
                }
                Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job, false, null, resultEncryption.Item2));
            }
            else
            {
                var resultCopy = FileUtils.CopyFile(fileInfo.FullName, job.DestinationPath,
                    Path.GetDirectoryName(fileInfo.FullName));
                if (!resultCopy.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Copy failed : {job.DestinationPath}", job, true));
                    throw new Exception(Errors.FileCantBeCopied);
                }
                Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job, false,resultCopy.Item2, null));
            }

            job.State.Progression = 100;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    public async Task ExecuteAsync(BackupJob job, List<string> priorityExtensions)
    {
        var cryptedExtensions = SettingsService.GetInstance.Settings.CryptExtensions;
    
        if (File.Exists(job.SourcePath))
        {
            await ProcessFileAsync(job, cryptedExtensions);
        }
        else if (Directory.Exists(job.SourcePath))
        {
            await ProcessDirectoryAsync(job, cryptedExtensions, priorityExtensions);
        }
        else
        {
            throw new FileNotFoundException(Errors.ProcessingError);
        }
    }

    private static async Task ProcessFileAsync(BackupJob job, List<string> cryptExt)
    {
        var fileInfo = new FileInfo(job.SourcePath);
        
        job.State.TotalFiles = 1;
        job.State.RemainingFiles = 1;
        job.State.FileSize = fileInfo.Length;
        job.State.RemainingFilesSize = fileInfo.Length;
        job.State.Progression = 0;
        
        if (cryptExt.Contains(fileInfo.Extension))
        {
            var sourcePath = fileInfo.FullName;
            var sourceRoot = Path.GetDirectoryName(sourcePath);
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot) ? fileInfo.Name : Path.GetRelativePath(sourceRoot, sourcePath);
            var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

            var resultEncryption = await Task.Run(() => CryptoUtils.EncryptFile(sourcePath, destinationFilePath));
            if (!resultEncryption.Item1)
            {
                Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destinationFilePath)}", job, true));
                throw new Exception(Errors.FileCantBeCrypted);
            }
            Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job, false, null, resultEncryption.Item2));
        }
        else
        {
            var resultCopy = await Task.Run(() => FileUtils.CopyFile(fileInfo.FullName, job.DestinationPath,
                Path.GetDirectoryName(fileInfo.FullName)));
            if (!resultCopy.Item1)
            {
                Logger.Instance.Write(new LogEntry($"Copy failed : {job.DestinationPath}", job, true));
                throw new Exception(Errors.FileCantBeCopied);
            }
            Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job, false, resultCopy.Item2, null));
        }

        job.State.Progression = 100;
    }

    private static async Task ProcessDirectoryAsync(BackupJob job, List<string> cryptExt, List<string> priorityExt)
    {
        var directoryInfo = new DirectoryInfo(job.SourcePath);
        var destinationBackupFolder = Path.Combine(job.DestinationPath, Path.GetFileName(job.SourcePath));
        
        Directory.CreateDirectory(destinationBackupFolder);
        
        var allFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        
        var (priorityFiles, normalFiles) = await Task.Run(() =>
        {
            var priority = new List<FileInfo>();
            var normal = new List<FileInfo>();
            
            Parallel.ForEach(allFiles, file =>
            {
                if (priorityExt.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                    lock (priority) priority.Add(file);
                else
                    lock (normal) normal.Add(file);
            });
            
            return (priority, normal);
        });

        job.State.TotalFiles = allFiles.Length;
        job.State.FileSize = allFiles.Sum(f => f.Length);
        job.State.RemainingFiles = allFiles.Length;
        job.State.RemainingFilesSize = job.State.FileSize;
        job.State.Progression = 0;

        // Priority's first
        await ProcessFilesInParallel(priorityFiles, job, destinationBackupFolder, cryptExt);
        await ProcessFilesInParallel(normalFiles, job, destinationBackupFolder, cryptExt);

        job.State.Progression = 100;
    }

    private static async Task ProcessFilesInParallel(List<FileInfo> files, BackupJob job, string destFolder, List<string> cryptExt)
    {
        await Parallel.ForEachAsync(files, async (file, ct) =>
        {
            var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
            var destinationFilePath = Path.Combine(destFolder, relativePath);
            var dirName = Path.GetDirectoryName(destinationFilePath);
            
            if (!string.IsNullOrEmpty(dirName))
                Directory.CreateDirectory(dirName);

            if (cryptExt.Contains(file.Extension))
            {
                var resultEncryption = await Task.Run(() => CryptoUtils.EncryptFile(file.FullName, destinationFilePath));
                if (!resultEncryption.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destinationFilePath)}", job, true));
                    throw new Exception(Errors.FileCantBeCrypted);
                }
                Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job, false, null, resultEncryption.Item2));
            }
            else
            {
                var resultCopy = await Task.Run(() => FileUtils.CopyFile(file.FullName, destFolder, job.SourcePath));
                if (!resultCopy.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Copy failed : {job.DestinationPath}", job, true));
                    throw new Exception(Errors.FileCantBeCopied);
                }
                Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job, false, resultCopy.Item2, null));
            }

            lock (job.State)
            {
                job.State.RemainingFiles--;
                job.State.RemainingFilesSize -= file.Length;
                job.State.Progression = (int)(100.0 * (1.0 - ((double)job.State.RemainingFilesSize / job.State.FileSize)));
            }
        });
    }

    private static bool ProcessDirectory(BackupJob job, List<string> cryptExt)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(job.SourcePath);
            var destinationBackupFolder = Path.Combine(job.DestinationPath, Path.GetFileName(job.SourcePath));
            
            Directory.CreateDirectory(destinationBackupFolder);
            
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories); 

            job.State.TotalFiles = files.Length;
            job.State.FileSize = job.State.FileSize = files.Sum(f => f.Length);
            job.State.RemainingFiles = files.Length;
            job.State.RemainingFilesSize = job.State.FileSize = files.Sum(f => f.Length);
            job.State.Progression = 0;

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);
                
                var dirName = Path.GetDirectoryName(destinationFilePath);

                if (string.IsNullOrEmpty(dirName))
                {
                    throw new Exception();
                }

                Directory.CreateDirectory(dirName);

                if (cryptExt.Contains(file.Extension))
                {
                    var resultEncryption = CryptoUtils.EncryptFile(file.FullName, destinationFilePath);
                    if (!resultEncryption.Item1)
                    {
                        Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destinationFilePath)}", job, true));
                        throw new Exception(Errors.FileCantBeCrypted);
                    }
                    Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job,false, null, resultEncryption.Item2));
                }
                else
                {
                    var resultCopy = FileUtils.CopyFile(file.FullName, destinationBackupFolder, job.SourcePath);
                    if (!resultCopy.Item1)
                    {
                        Logger.Instance.Write(new LogEntry($"Copy failed : {job.DestinationPath}", job, true));
                        throw new Exception(Errors.FileCantBeCopied);
                    }
                    Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job,false, resultCopy.Item2, null));
                }
                
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