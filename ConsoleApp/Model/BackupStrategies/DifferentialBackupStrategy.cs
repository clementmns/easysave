using EasyLog;
using EasySave.ConsoleApp.Utils;

namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class DifferentialBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        job.State.IsActive = true;
        job.State.Progression = 0;

        var result = (File.Exists(job.SourcePath), Directory.Exists(job.SourcePath)) switch
        {
            (true, false) => ProcessFile(job),
            (false, true) => ProcessDirectory(job),
            _ => throw new FileNotFoundException(Ressources.Errors.ProcessingError)
        };

        job.State.Reset();

        return result;
    }

    private static bool ProcessFile(BackupJob job)
    {
        try
        {
            var sourceFile = new FileInfo(job.SourcePath);
            var sourceRoot = Path.GetDirectoryName(sourceFile.FullName);
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot)
                ? sourceFile.Name
                : Path.GetRelativePath(sourceRoot, sourceFile.FullName);
            var destinationFilePath = Path.Combine(job.DestinationPath, relativePath);

            var shouldCopy = !File.Exists(destinationFilePath) || sourceFile.LastWriteTime > File.GetLastWriteTime(destinationFilePath);

            if (!shouldCopy)
            {
                job.State.TotalFiles = 0;
                job.State.RemainingFiles = 0;
                job.State.FileSize = 0;
                job.State.RemainingFilesSize = 0;
                return true;
            }

            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = sourceFile.Length;
            job.State.RemainingFilesSize = sourceFile.Length;

            if (!FileUtils.CopyFile(sourceFile.FullName, job.DestinationPath, Path.GetDirectoryName(sourceFile.FullName)))
            {
                throw new Exception(Ressources.Errors.FileCantBeCopied);
            }

            job.State.RemainingFiles = 0;
            job.State.RemainingFilesSize = 0;

            return true;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return false;
        }
    }

    private static bool ProcessDirectory(BackupJob job)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(job.SourcePath);
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            
            string destinationBackupFolder = Path.Combine(job.DestinationPath, Path.GetFileName(job.SourcePath) + "_copy");
            
            Directory.CreateDirectory(destinationBackupFolder);

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

            foreach (var file in filesToCopy)
            {
                var relativePath = Path.GetRelativePath(job.SourcePath, file.FullName);
                var destinationFilePath = Path.Combine(destinationBackupFolder, relativePath);
            
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
                
                if (!FileUtils.CopyFile(file.FullName, destinationBackupFolder, job.SourcePath))
                {
                    throw new Exception(Ressources.Errors.FileCantBeCopied);
                }

                job.State.RemainingFiles -= 1;
                job.State.RemainingFilesSize -= file.Length;
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return false;
        }
    }
}
