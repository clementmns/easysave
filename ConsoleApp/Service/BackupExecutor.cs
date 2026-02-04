using EasySave.ConsoleApp.Model;

namespace EasySave.ConsoleApp.Service;

public class BackupExecutor
{
    public void ExecuteJob(BackupJob job)
    {
        if (!Directory.Exists(job.SourcePath))
        {
            Console.WriteLine($"The source path {job.SourcePath} don't exist");
            return;
        }
        string[] files = Directory.GetFiles(job.SourcePath, "*", SearchOption.AllDirectories); // Find all the files even in the subfolders
        job.State.TotalFiles = files.Length;
        job.State.RemainingFiles = files.Length;
        job.State.IsActive = true;
        job.State.Progression = 0;
        
        long totalSize = 0;
        foreach (string file in files)
        {
            totalSize += new FileInfo(file).Length;
        }
        
        // Add copy of files
        
        foreach (string file in files)
        {
            long currentFileSize = new FileInfo(job.SourcePath).Length;
            job.State.RemainingFiles--;
            job.State.FileSize -= currentFileSize;
        
            if (job.State.FileSize > 0)
            {
                double percent = 100.0 * (1.0 - ((double)job.State.RemainingFilesSize / job.State.FileSize));
                job.State.Progression = (int)percent;
            }
            else
            {
                job.State.Progression = 100;
            }
        }
        
        job.State.IsActive = false; 
    }
}