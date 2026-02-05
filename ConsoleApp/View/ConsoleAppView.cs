using System.Reflection;
using System.Runtime.Versioning;
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
        Console.WriteLine(@" 2. " + Messages.ResourceManager.GetString("ConsoleMenuAddJob"));
        Console.WriteLine(@" 3. " + Messages.ResourceManager.GetString("ConsoleMenuDeleteJob"));
        Console.WriteLine(@" 4. " + Messages.ResourceManager.GetString("ConsoleMenuExecuteJob"));
        Console.WriteLine(@" 5. " + Messages.ResourceManager.GetString("ConsoleMenuExecuteAllJobs"));
        Console.WriteLine(@" 6. " + Messages.ResourceManager.GetString("ConsoleMenuLanguage"));
        Console.WriteLine(@" Q. " + Messages.ResourceManager.GetString("ConsoleMenuQuit"));
        Console.WriteLine(Messages.ResourceManager.GetString("ConsoleMenuOption"));
    }

    public void Run()
    {
        var exit = false;
        int maxFiles = 5;

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
                    var currentJobs = _viewModel.Jobs?.ToList() ?? [];
                    if (currentJobs.Count == maxFiles)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Messages.ResourceManager.GetString("MaxFileWarning"));
                        Console.ResetColor();
                        
                        DeleteJob();
                        AddJob();
                    }
                    else
                    {
                        AddJob();
                    }
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
                    break;}

            if (exit) break;
            Console.WriteLine();
            Console.WriteLine(Messages.ResourceManager.GetString("PressKeyToContinue"));
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ViewJobs()
    {
        if (_viewModel.Jobs == null) return;
        var jobs = _viewModel.Jobs.ToList();
        Console.WriteLine(Messages.ResourceManager.GetString("ViewJobsTitle"));

        if (jobs.Count == 0)
        {
            Console.WriteLine(Messages.ResourceManager.GetString("ViewJobsNoJob"));
        }
        else
        {
            for (var i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];
                Console.WriteLine(
                    $"{i + 1}. {job.Name} ({job.SourcePath} -> {job.DestinationPath}) - Type: {job.Type}");
            }
        }
    }

    private void AddJob()
    {
        Console.WriteLine(Messages.ResourceManager.GetString("AddJobName"));
        var name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine(Messages.ResourceManager.GetString("AddJobSourcePath"));
        var sourcePath = Console.ReadLine() ?? string.Empty;

        Console.WriteLine(Messages.ResourceManager.GetString("AddJobDestinationPath"));
        var destinationPath = Console.ReadLine() ?? string.Empty;

        Console.WriteLine(Messages.ResourceManager.GetString("AddJobSaveType"));
        Console.WriteLine(Messages.ResourceManager.GetString("AddJobTypeDifferential"));
        Console.WriteLine(Messages.ResourceManager.GetString("AddJobTypeFull"));

        var saveTypeInput = Console.ReadKey().KeyChar.ToString().ToUpper();

        var saveType = saveTypeInput switch
        {
            "1" => BackupType.Differential,
            "2" => BackupType.Full,
            _ => BackupType.Full,
        };

        BackupJob job;
        var currentJobs = _viewModel.Jobs?.ToList() ?? [];
        try
        {
            job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, saveType, currentJobs);
        }
        catch (Exception e)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("AddJobFailed"));
            return;
        }

        Console.Clear();
        Console.WriteLine(_viewModel.AddJob(job) 
            ? Messages.ResourceManager.GetString("AddJobSuccess") 
            : Messages.ResourceManager.GetString("AddJobFailed"));
    }

    private void DeleteJob() 
    {
        Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobPrompt"));
        ViewJobs();
    
        var jobs = _viewModel.Jobs?.ToList();
    
        if (jobs == null || jobs.Count == 0)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ViewJobsNoJob"));
            return;
        }

        var input = Console.ReadKey().KeyChar.ToString();
    
        if (int.TryParse(input, out var jobNumber) && jobNumber > 0 && jobNumber <= jobs.Count)
        {
            var job = jobs[jobNumber - 1];
            Console.Clear();

            Console.WriteLine(_viewModel.DeleteJob(job)
                ? Messages.ResourceManager.GetString("DeleteJobSuccess")
                : Messages.ResourceManager.GetString("DeleteJobFailed"));
        }
        else
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobInvalid"));
        }
    }


    private void ExecuteJobs()
    {
        ViewJobs();
        Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsPrompt"));
        var jobsList = _viewModel.Jobs?.ToList();
        
        if (jobsList == null || jobsList.Count == 0)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }
        
        var input = Console.ReadLine();

        var selectedJobs = new List<BackupJob>();
        var parts = input?.Split(';').Select(p => p.Trim()).ToArray();

        if (parts != null)
        {
            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (!int.TryParse(range[0], out var start) || !int.TryParse(range[1], out var end)) continue;
                    for (var i = start; i <= end && i <= jobsList.Count; i++)
                    {
                        selectedJobs.Add(jobsList[i - 1]);
                    }
                }
                else if (int.TryParse(part, out var jobNumber) && jobNumber > 0 && jobNumber <= jobsList.Count)
                {
                    selectedJobs.Add(jobsList[jobNumber - 1]);
                }
            }
        }

        Console.Clear();
        if (selectedJobs.Count > 0)
        {
            foreach (var job in selectedJobs)
            {
                _viewModel.ExecuteJob(job);
            }

            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsSuccess"));
        }
        else
        {
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
        }
    }

    private void ExecuteAllJobs()
    {
        if (_viewModel.Jobs != null && _viewModel.Jobs.Count == 0) 
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }
        
        if (_viewModel.Jobs != null)
            foreach (var job in _viewModel.Jobs.ToList())
                _viewModel.ExecuteJob(job);
        
        Console.Clear();
        Console.WriteLine(Messages.ResourceManager.GetString("ExecuteAllJobsSuccess"));
    }

    private void ChangeLanguage()
    {
        Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageTitle"));
        Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageEnglish"));
        Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageFrench"));

        var langInput = Console.ReadKey().KeyChar.ToString();
        
        string language;
        Console.Clear();
        switch (langInput)

        {
            case "1": 
                language = "en-US";
                Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
                break;
            case "2":
                language = "fr-FR";
                Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
                break;
            default:
                Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageInvalid"));
                language = "en-US";
                break;
        }
        

        SettingsService.GetInstance.SetLanguage(language);
    }
}