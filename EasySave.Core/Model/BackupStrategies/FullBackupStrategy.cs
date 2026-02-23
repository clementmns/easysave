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

    private static void CopyOrEncryptFile(string sourceFile, string destFile, string sourceRoot, string destFolder, List<string> cryptExt, BackupJob job)
    {
        var fileInfo = new FileInfo(sourceFile);
        var dirName = Path.GetDirectoryName(destFile);

        if (!string.IsNullOrEmpty(dirName))
            Directory.CreateDirectory(dirName);

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
            
            var sourcePath = fileInfo.FullName;
            var sourceRoot = Path.GetDirectoryName(sourcePath);
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot) ? fileInfo.Name : Path.GetRelativePath(sourceRoot, sourcePath);
            var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

            CopyOrEncryptFile(sourcePath, destinationFilePath, sourceRoot ?? string.Empty, job.DestinationPath, cryptExt, job);

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

            job.State.TotalFiles = files.Length;
            job.State.FileSize = files.Sum(f => f.Length);
            job.State.RemainingFiles = files.Length;
            job.State.RemainingFilesSize = job.State.FileSize;
            job.State.Progression = 0;

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);

                CopyOrEncryptFile(file.FullName, destinationFilePath, job.SourcePath, destinationBackupFolder, cryptExt, job);
                
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