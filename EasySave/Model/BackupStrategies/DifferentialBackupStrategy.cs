using EasySave.Utils;

namespace EasySave.Model.BackupStrategies;

public sealed class DifferentialBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));

        var files = FileUtils.GetAllFiles(job.SourcePath);
        var filesToCopy = files.Where(file => ShouldCopy(file, job)).ToList();

        job.State.TotalFiles = filesToCopy.Count;
        job.State.FileSize = filesToCopy.Sum(file => file.Length);
        job.State.RemainingFiles = job.State.TotalFiles;
        job.State.RemainingFilesSize = job.State.FileSize;
        job.State.Progression = 0;
        job.State.IsActive = true;

        var copiedFiles = 0;
        var remainingSize = job.State.FileSize;

        foreach (var file in filesToCopy)
        {
            FileUtils.CopyFile(file.FullName, job.DestinationPath, job.SourcePath);
            copiedFiles++;
            remainingSize -= file.Length;

            job.State.RemainingFiles = job.State.TotalFiles - copiedFiles;
            job.State.RemainingFilesSize = Math.Max(0, remainingSize);
            job.State.Progression = job.State.TotalFiles == 0
                ? 100
                : (int)Math.Round(copiedFiles * 100.0 / job.State.TotalFiles);
            job.State.LastUpdate = DateTime.Now;
        }

        job.State.IsActive = false;
        return true;
    }

    private static bool ShouldCopy(FileInfo sourceFile, BackupJob job)
    {
        var relativePath = Path.GetRelativePath(job.SourcePath, sourceFile.FullName);
        var destinationFile = Path.Combine(job.DestinationPath, relativePath);

        if (!File.Exists(destinationFile)) return true;

        var destinationLastWrite = FileUtils.GetLastModifiedDate(destinationFile);
        if (destinationLastWrite == null) return true;

        return sourceFile.LastWriteTime > destinationLastWrite.Value;
    }
}
