using static System.FormattableString;

using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Ressources;
using EasySave.ConsoleApp.Service;
using EasySave.ConsoleApp.ViewModel;

namespace EasySave.ConsoleApp.View;

public class ConsoleAppView
{
    private readonly BackupViewModel _backupViewModel;

    public ConsoleAppView(string appSaveDirectory)
    {
        _backupViewModel = new BackupViewModel(appSaveDirectory);
    }

    public void RunWithArgs(string[] args)
    {
        var executed = _backupViewModel.ExecuteJobsFromArgs(args[0]);
        Console.WriteLine(executed
            ? Messages.ResourceManager.GetString("ExecuteJobsSuccess")
            : Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
    }

    private static void ShowHeader()
    {
        Console.WriteLine(@" ______                 _____
|  ____|               / ____|
| |__   __ _ ___ _   _| (___   __ ___   _____
|  __| / _` / __| | | |\___ \ / _` \ \ / / _ \
| |___| (_| \__ \ |_| |____) | (_| |\ V /  __/
|______\__,_|___/\__, |_____/ \__,_| \_/ \___|
                  __/ |
                 |___/");
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
        if (_backupViewModel.Jobs == null) return;
        var jobs = _backupViewModel.Jobs.ToList();
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
                    $@"{i + 1}. {job.Name} ({job.SourcePath} -> {job.DestinationPath}) - Type: {job.Type}");
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
            _ => BackupType.Full
        };

        BackupJob job;
        var currentJobs = _backupViewModel.Jobs?.ToList() ?? [];
        try
        {
            job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, saveType, currentJobs);
        }
        catch
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("AddJobFailed"));
            return;
        }

        Console.Clear();
        Console.WriteLine(_backupViewModel.AddJob(job) 
            ? Messages.ResourceManager.GetString("AddJobSuccess") 
            : Messages.ResourceManager.GetString("AddJobFailed"));
    }

    private void DeleteJob() 
    {
        Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobPrompt"));
        ViewJobs();
    
        var jobs = _backupViewModel.Jobs?.ToList();
    
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

            Console.WriteLine(_backupViewModel.DeleteJob(job)
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
        var jobsList = _backupViewModel.Jobs?.ToList();
        
        if (jobsList == null || jobsList.Count == 0)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }
        
        string? input = Console.ReadLine();
        
        if (!int.TryParse(input, out int userInput) || userInput < 1 || userInput > jobsList.Count)
        {
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
            return;
        }

        int index = userInput - 1;
        var selectedJob = jobsList[index];

        Console.Clear();
    
        try
        {
            _backupViewModel.ExecuteJob(selectedJob);
    
            Console.WriteLine(@"");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Invariant($"\r ✓ Sauvegarde complétée!"));
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Invariant($"\r ✗ Erreur: {ex.Message}"));
            Console.ResetColor();
        }
        finally
        {
            Console.WriteLine(@"Appuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }

    private void ExecuteAllJobs()
    {
        if (_backupViewModel.Jobs != null && _backupViewModel.Jobs.Count == 0) 
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }
        
        if (_backupViewModel.Jobs != null)
            foreach (var job in _backupViewModel.Jobs.ToList())
                _backupViewModel.ExecuteJob(job);
        
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
