using EasySave.ConsoleApp.Utils;
using EasyLog;

namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class FullBackupStrategy : IBackupStrategy
{
    public bool Execute(BackupJob job)
    {
        job.State.IsActive = true;
        job.State.Progression = 0;
        
        var result = (File.Exists(job.SourcePath), Directory.Exists(job.DestinationPath)) switch
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
            FileInfo fileInfo = new FileInfo(job.SourcePath);

            job.State.TotalFiles = 1;
            job.State.RemainingFiles = 1;
            job.State.FileSize = fileInfo.Length;
            job.State.RemainingFilesSize = fileInfo.Length;

            if (!FileUtils.CopyFile(fileInfo.FullName, job.DestinationPath))
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
            DirectoryInfo directoryInfo = new DirectoryInfo(job.SourcePath);
            // Find all the files even in the subfolders
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories); 

            job.State.TotalFiles = files.Length;
            job.State.FileSize = job.State.FileSize = files.Sum(f => f.Length);
            job.State.RemainingFiles = files.Length;
            job.State.RemainingFilesSize = job.State.FileSize = files.Sum(f => f.Length);

            foreach (FileInfo f in files)
            {
                if (!FileUtils.CopyFile(f.FullName, job.DestinationPath)) // Attention : cette méthode peut tout enregistrer dans le même dossier et ne pas prendre en compte les sous-dossiers. en gros ne pas reproduire l'arborescence => copy
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