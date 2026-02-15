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
        job.State.Status = RealTimeState.RealTimeStatus.OnGoing;
        job.State.IsActive = true;
        job.State.Progression = 0;

        var cryptedExtensions = SettingsService.GetInstance.Settings.CryptExtensions;

        var result = (File.Exists(job.SourcePath), Directory.Exists(job.SourcePath)) switch
        {
            (true, false) => ProcessFile(job, cryptedExtensions),
            (false, true) => ProcessDirectory(job,  cryptedExtensions),
            _ => throw new FileNotFoundException(Errors.ProcessingError)
        };

        job.State.Status = result ? RealTimeState.RealTimeStatus.Done : RealTimeState.RealTimeStatus.Error;
        job.State.Reset();

        return result;
    }
    
    private static bool ProcessFile(BackupJob job, List<string> cryptExt)
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

            if (cryptExt.Contains(fileInfo.Extension))
            {
                var resultEncryption = CryptoUtils.EncryptFile(sourcePath, destinationFilePath);
                if (!resultEncryption.Item1)
                {
                    Logger.Instance.Write(new LogEntry($"Encryption failed : {Path.GetFileName(destinationFilePath)}", job, true));
                    throw new Exception(Errors.FileCantBeCrypted);
                }
                Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job,false, resultEncryption.Item2));
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
                Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job,false, resultCopy.Item2));
            }
        
            job.State.Progression = 100;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool ProcessDirectory(BackupJob job, List<string> cryptExt)
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

            job.State.TotalFiles = filesToCopy.Count;
            job.State.FileSize = filesToCopy.Sum(f => f.Length);
            job.State.RemainingFiles = filesToCopy.Count;
            job.State.RemainingFilesSize = job.State.FileSize;
            job.State.Progression = 0;

            foreach (var file in filesToCopy)
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
                    Logger.Instance.Write(new LogEntry($"File Encrypted : {job.DestinationPath}", job,false, resultEncryption.Item2));
                }
                else
                {
                    var resultCopy = FileUtils.CopyFile(file.FullName, destinationBackupFolder, job.SourcePath);
                    if (!resultCopy.Item1)
                    {
                        Logger.Instance.Write(new LogEntry($"Copy failed : {job.DestinationPath}", job, true));
                        throw new Exception(Errors.FileCantBeCopied);
                    }
                    Logger.Instance.Write(new LogEntry($"File Copied : {job.DestinationPath}", job,false, resultCopy.Item2));
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