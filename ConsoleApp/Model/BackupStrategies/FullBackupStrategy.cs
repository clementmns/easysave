using EasySave.ConsoleApp.Utils;

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
            job.State.Progression = 0;

            if (!FileUtils.CopyFile(fileInfo.FullName, job.DestinationPath, Path.GetDirectoryName(fileInfo.FullName)))
            {
                throw new Exception(Ressources.Errors.FileCantBeCopied);
            }

            job.State.Progression = 100;
            job.State.Reset();
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool ProcessDirectory(BackupJob job)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(job.SourcePath + "");
            string destinationBackupFolder = Path.Combine(job.DestinationPath, Path.GetFileName(job.SourcePath) + "_copy");
            
            Directory.CreateDirectory(destinationBackupFolder);
            
            // Find all the files even in the subfolders
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
                
                if (!FileUtils.CopyFile(file.FullName, destinationBackupFolder, directoryInfo.FullName))
                {
                    throw new Exception(Ressources.Errors.FileCantBeCopied);
                }

                job.State.RemainingFiles -= 1;
                job.State.RemainingFilesSize -= file.Length;
                job.State.Progression = (int)(100.0 * (1.0 - ((double)job.State.RemainingFilesSize / job.State.FileSize)));
            }
            
            job.State.Progression = 100;
            job.State.Reset();
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
