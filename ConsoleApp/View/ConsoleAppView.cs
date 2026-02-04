using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Ressources;
using EasySave.ConsoleApp.Service;
using EasySave.ConsoleApp.ViewModel;

namespace EasySave.ConsoleApp.View;

public class ConsoleAppView
{
    private readonly BackupViewModel _viewModel;

    public ConsoleAppView(string appSaveDirectory)
    {
        _viewModel = new BackupViewModel(appSaveDirectory);
    }

    private static void ShowHeader()
    {
       Console.WriteLine("""
          ______                 _____                 
         |  ____|               / ____|                
         | |__   __ _ ___ _   _| (___   __ ___   _____ 
         |  __| / _` / __| | | |\___ \ / _` \ \ / / _ \
         | |___| (_| \__ \ |_| |____) | (_| |\ V /  __/
         |______\__,_|___/\__, |_____/ \__,_| \_/ \___|
                           __/ |                       
                          |___/                        
         """);
    }

    private static void ShowMenu()
    {
        Console.WriteLine(@" 1. " + Messages.ResourceManager.GetString("ConsoleMenuViewJobs"));
        Console.WriteLine(" 2. Add Job");
        Console.WriteLine(" 3. Delete Job");
        Console.WriteLine(" 4. Execute one or more jobs");
        Console.WriteLine(" 5. Execute all jobs");
        Console.WriteLine(" 6. Change Language");
        Console.WriteLine(" Q. Quit");
        Console.WriteLine("Choose an Option : ");
    }

    public void Run()
    {
        var exit = false;

        while (!exit)
        {
            ShowHeader();
            ShowMenu();

            var choice = Console.ReadKey().KeyChar.ToString().ToUpper();
            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ViewJobs();
                    break;

                case "2":
                    Console.Clear();
                    AddJob();
                    break;

                case "3":
                    Console.Clear();
                    DeleteJob();
                    break;

                case "4":
                    Console.Clear();
                    ExecuteJobs();
                    break;

                case "5":
                    Console.Clear();
                    ExecuteAllJobs();
                    break;

                case "6":
                    Console.Clear();
                    ChangeLanguage();
                    break;

                case "Q":
                    exit = true;
                    break;
            }
            
            if (exit) break;
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ViewJobs()
    {
        if (_viewModel.Jobs == null) return;
        var jobs = _viewModel.Jobs.ToList();
        Console.WriteLine("Here the jobs : ");
        
        if (jobs.Count == 0)
        {
            Console.WriteLine("No job available.");
        }
        else
        {
            for (var i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];
                Console.WriteLine($"{i + 1}. {job.Name} ({job.SourcePath} -> {job.DestinationPath}) - Type: {job.Type}");
            }
        }
    }

    private void AddJob()
    {
        Console.WriteLine("Add a job name :");
        var name = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Add a Path (source) :");
        var sourcePath = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Add a Path (destination) :");
        var destinationPath = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Define the type of save that you want  : Differential | Complete"); 
        var typeInput = Console.ReadLine() ?? string.Empty;
        
        // TODO: Use Switch for available backup types (Differential, Complete for the moment)

        if (!Enum.TryParse<BackupType>(typeInput, true, out var backupType))
        {
            Console.WriteLine("Type invalid. Switch to complete by default.");
            backupType = BackupType.Full;
        }
        var job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, backupType);

        Console.WriteLine(_viewModel.AddJob(job) ? "Job add with success." : "Job add failed.");
    }

    private void DeleteJob()
    {
        Console.Write("Enter the job to remove : ");
        ViewJobs();
        var name = Console.ReadLine() ?? string.Empty;
        var jobs = _viewModel.Jobs?.ToList();
        var job = jobs?.Find(j => j.Name == name);

        if (job != null && _viewModel.DeleteJob(job))
        {
            Console.WriteLine("Job removed with success");
        }
        else
        {
            Console.WriteLine("Remove of the job failed.");
        }
    }

    private void ExecuteJobs()
    {
        Console.Write("Enter the name of the jobs to execute : ");
        
        // TODO: Use ViewJobs() to get the list of jobs and select one or more jobs to execute (1,3 = 1 to 3) (1;3 = 1 and 3)
        var input = Console.ReadLine();
        var names = input?.Split(',').Select(n => n.Trim()).ToArray();
        var jobs = _viewModel.Jobs?.ToList().FindAll(j => names?.Contains(j.Name) == true);

        if (jobs != null && jobs.Count > 0)
        {
            foreach (var job in jobs)
            {
                _viewModel.ExecuteJob(job);
            }
            Console.WriteLine("Jobs successfully completed.");
        }
        else
        {
            Console.WriteLine("No jobs found with these names.");
        }
    }

    private void ExecuteAllJobs()
    {
        if (_viewModel.Jobs != null) foreach (var job in _viewModel.Jobs.ToList()) _viewModel.ExecuteJob(job);
        Console.WriteLine("All jobs have been successfully completed.");
    }

    private void ChangeLanguage()
    {
        Console.WriteLine("Change the language : ");
        Console.WriteLine("1. English");
        Console.WriteLine("2. French");
        var langInput = Console.ReadKey().KeyChar.ToString();
        
        var language = langInput switch
        {
            "1" => "en-US",
            "2" => "fr-FR",
            _ => "en-US" // TODO: Tell that in default case, because the Input var undefined, we used English
        };
        SettingsService.GetInstance.SetLanguage(language);
    }
}
