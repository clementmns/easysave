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
        Console.ForegroundColor = ConsoleColor.Green;
        string[] logo = {
            "███████╗ █████╗ ███████╗██╗   ██╗███████╗ █████╗ ██╗   ██╗███████╗",
            "██╔════╝██╔══██╗██╔════╝╚██╗ ██╔╝██╔════╝██╔══██╗██║   ██║██╔════╝",
            "█████╗  ███████║███████╗ ╚████╔╝ ███████╗███████║██║   ██║█████╗  ",
            "██╔══╝  ██╔══██║╚════██║  ╚██╔╝  ╚════██║██╔══██║╚██╗ ██╔╝██╔══╝  ",
            "███████╗██║  ██║███████║   ██║   ███████║██║  ██║ ╚████╔╝ ███████╗",
            "╚══════╝╚═╝  ╚═╝╚══════╝   ╚═╝   ╚══════╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝"
        };
        foreach (string line in logo)
        {
            Console.WriteLine(line);
        }
        
        Console.ResetColor();
    }
    
    public void Run()
    {
        var exit = false;
        int maxFiles = 5;

        while (!exit)
        {
            string[] options =
            {
                Messages.ResourceManager.GetString("ConsoleMenuViewJobs"), 
                Messages.ResourceManager.GetString("ConsoleMenuAddJob"), 
                Messages.ResourceManager.GetString("ConsoleMenuDeleteJob"), 
                Messages.ResourceManager.GetString("ConsoleMenuExecuteJob"), 
                Messages.ResourceManager.GetString("ConsoleMenuExecuteAllJobs"), 
                Messages.ResourceManager.GetString("ConsoleMenuLanguage"), 
                Messages.ResourceManager.GetString("ConsoleMenuQuit")
            };
            
            int choice = NavigateMenu(options);
            Console.Clear();
            
            switch (choice)
            {
                case 0:
                    Console.Clear();
                    ViewJobs();
                    break;

                case 1:
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

                case 2:
                    Console.Clear();
                    DeleteJob();
                    break;

                case 3:
                    Console.Clear();
                    ExecuteJobs();
                    break;

                case 4:
                    Console.Clear();
                    ExecuteAllJobs();
                    break;

                case 5:
                    Console.Clear();
                    ChangeLanguage();
                    break;

                case 6:
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
        
        string[] options = {
            Messages.ResourceManager.GetString("AddJobTypeDifferential"),
            Messages.ResourceManager.GetString("AddJobTypeFull")
        };
        string question = Messages.ResourceManager.GetString("AddJobSaveType");
        int selection = NavigateMenu(options, question);
        var saveType = selection == 0 ? BackupType.Differential : BackupType.Full;       
        
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
        var jobs = _viewModel.Jobs?.ToList();
    
        if (jobs == null || jobs.Count == 0)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ViewJobsNoJob"));
            return;
        }

        var deleteOptions = new List<string>();
        foreach (var job in jobs)
        {
            deleteOptions.Add($"{job.Name} ({job.Type})");
        }
        deleteOptions.Add(Messages.ResourceManager.GetString("ConsoleMenuQuit") ?? "Cancel");
        string title = Messages.ResourceManager.GetString("DeleteJobPrompt");
        int selection = NavigateMenu(deleteOptions.ToArray(), title);
        
        if (selection == deleteOptions.Count - 1)
        {
            return; 
        }
        var jobToDelete = jobs[selection];
        Console.Clear();
        
        bool success = _viewModel.DeleteJob(jobToDelete);
        
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobSuccess"));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobFailed"));
        }
        Console.ResetColor();
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
        string[] options = {
            Messages.ResourceManager.GetString("ChangeLanguageEnglish"),
            Messages.ResourceManager.GetString("ChangeLanguageFrench")   
        };
        string title = Messages.ResourceManager.GetString("ChangeLanguageTitle");
        int selection = NavigateMenu(options, title);
        string language;
        switch (selection)

        {
            case 0: 
                language = "en-US";
                break;
            case 1:
                language = "fr-FR";
                break;
            default:
                language = "en-US";
                break;
        }
        SettingsService.GetInstance.SetLanguage(language);
        Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
    }

    private int NavigateMenu(string[] options, string? question = null)
    {
        
        int selection = 0;
        Console.CursorVisible = false;

        while (true)
        {
            Console.Clear();
            ShowHeader();

            if (!string.IsNullOrWhiteSpace(question))
            {
                Console.WriteLine(question);
                Console.WriteLine();
            }
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"-> {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"{options[i]}");
                }
            }
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.DownArrow && selection < options.Length - 1)
            {
                selection++;
            }
            else if (key == ConsoleKey.UpArrow && selection > 0)
            {
                selection--;
            }
            else if (key == ConsoleKey.Enter)
            {
                Console.CursorVisible = true;
                return selection;
            }
        }
    }
}