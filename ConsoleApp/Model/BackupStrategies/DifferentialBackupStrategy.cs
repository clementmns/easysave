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
            var destinationFilePath = Path.Combine(job.DestinationPath, sourceFile.Name);

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

            List<FileInfo> filesToCopy = [];

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(job.DestinationPath, file.Name);
                if (!File.Exists(destinationFilePath) || file.LastWriteTime > File.GetLastWriteTime(destinationFilePath))
                {
                    filesToCopy.Add(file);
                }
            }

            job.State.TotalFiles = filesToCopy.Count;
            job.State.FileSize = filesToCopy.Sum(f => f.Length);
            job.State.RemainingFiles = filesToCopy.Count;
            job.State.RemainingFilesSize = job.State.FileSize;

            foreach (var f in filesToCopy)
            {
                if (!FileUtils.CopyFile(f.FullName, job.DestinationPath, job.SourcePath))
                {
                    throw new Exception(Ressources.Errors.FileCantBeCopied);
                }

                job.State.RemainingFiles -= 1;
                job.State.RemainingFilesSize -= f.Length;
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
