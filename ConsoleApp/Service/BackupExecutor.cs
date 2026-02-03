using EasySave.ConsoleApp.Model;

namespace EasySave.ConsoleApp.Service;

public class BackupExecutor
{
    public void ExecuteJob(BackupJob job)
    {
        if (!Directory.Exists(job._sourcePath))
        {
            Console.WriteLine($"The source path {job._sourcePath} don't exist");
            return;
        }
        string[] files = Directory.GetFiles(job._sourcePath, "*", SearchOption.AllDirectories); // Find all the files even in the subfolders
        job._state.TotalFiles = files.Length;
        job._state.RemainingFiles = files.Length;
        job._state.IsActive = true;
        job._state.Progression = 0;

        long totalSize = 0;
        foreach (string file in files)
        {
            totalSize += new FileInfo(file).Length;
        }
        job._state.Notify();
        
        // Add copy of files

        foreach (string file in files)
        {
            long currentFileSize = new FileInfo(job._sourcePath).Length;
            job._state.RemainingFiles--;
            job._state.FileSize -= currentFileSize;

            if (job._state.FileSize > 0)
            {
                double percent = 100.0 * (1.0 - ((double)job._state.RemainingFilesSize / job._state.FileSize));
                job._state.Progression = (int)percent;
            }
            else
            {
                job._state.Progression = 100;
            }
            job._state.Notify();
        }

        job._state.IsActive = false; 
        job._state.Notify();

    }
}