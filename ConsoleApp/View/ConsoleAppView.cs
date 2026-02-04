using EasySave.ConsoleApp.Model;
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

    public void ShowHeader()
    {
        Console.WriteLine("===================================");
        Console.WriteLine("        Easy Save Application      ");
        Console.WriteLine("===================================");
    }

    public void Run()
    {
        var exit = false;

        while (!exit)
        {
            ShowHeader();
            Console.WriteLine(" 1. View saved jobs");
            Console.WriteLine(" 2. Add Job");
            Console.WriteLine(" 3. Delete Job");
            Console.WriteLine(" 4. Execute one or more jobs");
            Console.WriteLine(" 5. Execute all jobs");
            Console.WriteLine(" 6. Change Language");
            Console.WriteLine(" Q. Quit");
            Console.WriteLine("Choose an Option : ");

            var choice = Console.ReadLine()?.ToUpper();
            switch (choice)
            {
                case "1":
                    ViewSavedJobs();
                    break;

                case "2":
                    AddJob();
                    break;

                case "3":
                    DeleteJob();
                    break;

                case "4":
                    ExecuteJobs();
                    break;

                case "5":
                    ExecuteAllJobs();
                    break;

                case "6":
                    ChangeLanguage();
                    break;

                case "Q":
                    exit = true;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    public void ViewSavedJobs()
    {
        if (_viewModel.Jobs != null)
        {
            var jobs = _viewModel.Jobs.ToList();
            Console.WriteLine("Here the jobs : ");
        
            if (jobs.Count == 0)
            {
                Console.WriteLine("No job available.");
            }
            else
            {
                foreach (var job in jobs)
                {
                    Console.WriteLine($"- {job.Name} ({job.SourcePath} -> {job.DestinationPath}) - Type: {job.Type}");
                }
            }
        }

        Console.ReadLine();
    }

    public void AddJob()
    {
        Console.WriteLine("Add a job");
        var name = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Add a Path (source)");
        var sourcePath = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Add a Path (destination)");
        var destinationPath = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("Define the type of save that you want  : Differential | Complete");
        var typeInput = Console.ReadLine() ?? string.Empty;

        if (!Enum.TryParse<BackupType>(typeInput, true, out var backupType))
        {
            Console.WriteLine("Type invalid. Switch to complete by default.");
            backupType = BackupType.Full;
        }
        var job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, backupType);

        Console.WriteLine(_viewModel.AddJob(job) ? "Job add with success." : "Job add failed.");
        Console.ReadLine();
    }

    private void DeleteJob()
    {
        Console.Write("Enter the job to remove : ");
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
        Console.ReadLine();
    }

    private void ExecuteJobs()
    {
        Console.Write("Enter the name of the jobs to execute : ");
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
        Console.ReadLine();
    }

    private void ExecuteAllJobs()
    {
        if (_viewModel.Jobs != null)
        {
            foreach (var job in _viewModel.Jobs.ToList())
            {
                _viewModel.ExecuteJob(job);
            }
        }

        Console.WriteLine("All jobs have been successfully completed.");
        Console.ReadLine();
    }

    private void ChangeLanguage()
    {
        Console.WriteLine("Change the language : ");
        Console.WriteLine("1. English");
        Console.WriteLine("2. French");
        var langInput = Console.ReadLine() ?? string.Empty;
        
        var language = langInput switch
        {
            "1" => "en-US",
            "2" => "fr-FR",
            _ => "en-US"
        };
        SettingsService.GetInstance.SetLanguage(language);
    }
}
