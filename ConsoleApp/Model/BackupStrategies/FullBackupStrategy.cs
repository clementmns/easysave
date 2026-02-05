using EasySave.ConsoleApp.Utils;
using EasyLog;

namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class FullBackupStrategy : IBackupStrategy
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
            var fileInfo = new FileInfo(job.SourcePath);

            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = fileInfo.Length;
            job.State.RemainingFilesSize = fileInfo.Length;

            if (!FileUtils.CopyFile(fileInfo.FullName, job.DestinationPath, Path.GetDirectoryName(fileInfo.FullName)))
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

    private bool ProcessDirectory(BackupJob job)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(job.SourcePath);
            // Find all the files even in the subfolders
            var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories); 

            job.State.TotalFiles = files.Length;
            job.State.FileSize = job.State.FileSize = files.Sum(f => f.Length);
            job.State.RemainingFiles = files.Length;
            job.State.RemainingFilesSize = job.State.FileSize = files.Sum(f => f.Length);

            foreach (var f in files)
            {
                if (!FileUtils.CopyFile(f.FullName, job.DestinationPath, directoryInfo.FullName))
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
