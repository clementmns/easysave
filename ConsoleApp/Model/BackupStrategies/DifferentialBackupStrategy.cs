using EasySave.ConsoleApp.Utils;
using EasyLog;

namespace EasySave.ConsoleApp.Model.BackupStrategies;

public class DifferentialBackupStrategy : IBackupStrategy
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

    private bool ProcessFile(BackupJob job)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(job.SourcePath);
            string backupFilePath = Path.Combine(job.DestinationPath, fileInfo.Name, "_copy");
            FileInfo backupFileInfo = new FileInfo(backupFilePath);

            // verifie si le dossier de destination contient une sauvegarde
            if (!backupFileInfo.Exists)
            {
                // si pas de sauvegarde complète, on la fait et on notifie qu'on a fait une complète à l'utilisateur
                // raise une erreur, puis traiter l'exception en appelant une fonction qui va faire la sauvegarde complète
                return new FullBackupStrategy().Execute(job);
            }
            
            if (fileInfo.LastWriteTime > backupFileInfo.LastWriteTime)
            {
                // fichier source modifié après la dernière backup (il faut backup)
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

            } // fichier identique, on laisse

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
            DirectoryInfo directoryInfo = new DirectoryInfo(job.SourcePath);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            
            job.State.TotalFiles = files.Length;
            job.State.FileSize = job.State.FileSize = files.Sum(f => f.Length);
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return false;
        }
    }
}