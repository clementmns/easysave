using EasySave.Model;
using EasySave.Service;
using EasySave.Ressources;
using EasySave.ViewModel;

namespace EasySave.View;

public class ConsoleAppView : IProgressionObserver
{
    private readonly BackupViewModel _backupViewModel;

    public ConsoleAppView(string appSaveDirectory)
    {
        _backupViewModel = new BackupViewModel(appSaveDirectory);
    }

    public void OnProgressionUpdated(int progression)
    {
        Console.Clear();
        ShowHeader();
        Console.WriteLine(@"Sauvegarde en cours...");
        
        const int barLength = 100;
        var filledLength = progression;
        var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
        
        Console.ForegroundColor = ConsoleTheme.MainColor;
        Console.WriteLine($@"[{bar}] {progression}%");
        Console.WriteLine();
        Console.ResetColor();
    }

    public void RunWithArgs(string[] args)
    {
        var executed = _backupViewModel.ExecuteJobsFromArgs(args[0]);
        foreach (var (requestedIndex, result) in executed)
        {
            Console.WriteLine(!result
                ? requestedIndex + " :" + Messages.ResourceManager.GetString("ExecuteJobsFailed")
                : requestedIndex + " :" + Messages.ResourceManager.GetString("ExecuteJobsSuccess"));
        }
    }

    private static void ShowHeader()
    {
        Console.ForegroundColor = ConsoleTheme.MainColor;
        string[] logo = AppLogo.Logo;
        foreach (string line in logo)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    public void Run()
    {
        var exit = false;
        const int maxFiles = 5;

        while (!exit)
        {
            string?[] options =
            [
                Messages.ResourceManager.GetString("ConsoleMenuViewJobs"),
                Messages.ResourceManager.GetString("ConsoleMenuAddJob"),
                Messages.ResourceManager.GetString("ConsoleMenuDeleteJob"),
                Messages.ResourceManager.GetString("ConsoleMenuExecuteJob"),
                Messages.ResourceManager.GetString("ConsoleMenuExecuteAllJobs"),
                Messages.ResourceManager.GetString("ConsoleMenuLanguage"),
                Messages.ResourceManager.GetString("ConsoleMenuPath"),
                Messages.ResourceManager.GetString("ConsoleMenuQuit")
            ];

            int choice = NavigateMenu(options);
            Console.Clear();
            ShowHeader();

            switch (choice)
            {
                case 0:
                    ViewJobs();
                    break;

                case 1:
                    var currentJobs = _backupViewModel.Jobs?.ToList() ?? [];
                    if (currentJobs.Count == maxFiles)
                    {
                        Console.ForegroundColor = ConsoleTheme.WarningColor;
                        Console.WriteLine(Messages.ResourceManager.GetString("MaxFileWarning"));
                        Console.ResetColor();

                        if (DeleteJob())
                        {
                            AddJob();
                        }
                    }
                    else
                    {
                        AddJob();
                    }
                    break;

                case 2:
                    DeleteJob();
                    break;

                case 3:
                    ExecuteJobs();
                    break;

                case 4:
                    ExecuteAllJobs();
                    break;

                case 5:
                    ChangeLanguage();
                    break;
                
                case 6:
                    Console.Clear();
                    ShowHeader();
                    AddToPath();
                    break;

                case 7:
                    Console.WriteLine(Messages.ResourceManager.GetString("ThankYouForUsing"));
                    exit = true;
                    break;
            }

            if (exit) break;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleTheme.InstructionColor;
            Console.WriteLine(Messages.ResourceManager.GetString("PressKeyToContinue"));
            Console.ResetColor();
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

        string?[] options =
        {
            Messages.ResourceManager.GetString("AddJobTypeDifferential"),
            Messages.ResourceManager.GetString("AddJobTypeFull")
        };
        string question = Messages.ResourceManager.GetString("AddJobSaveType");
        int selection = NavigateMenu(options, question);
        var saveType = selection == 0 ? BackupType.Differential : BackupType.Full;

        BackupJob job;
        var currentJobs = _backupViewModel.Jobs?.ToList() ?? [];
        try
        {
            job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, saveType, currentJobs);
        }
        catch (Exception e)
        {
            Console.Clear();
            ShowHeader();
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("AddJobFailed"));
            Console.ResetColor();
            return;
        }

        Console.Clear();
        ShowHeader();
        bool success = _backupViewModel.AddJob(job);
        Console.ForegroundColor = success ? ConsoleTheme.MainColor : ConsoleTheme.ErrorColor;
        Console.WriteLine(success 
            ? Messages.ResourceManager.GetString("AddJobSuccess") 
            : Messages.ResourceManager.GetString("AddJobFailed"));
        Console.ResetColor();
    }

    private bool DeleteJob() 
    {
        var jobs = _backupViewModel.Jobs?.ToList();

        if (jobs == null || jobs.Count == 0)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(Messages.ResourceManager.GetString("ViewJobsNoJob"));
            return false;
        }

        var deleteOptions = new List<string>();
        foreach (var job in jobs)
        {
            deleteOptions.Add($"{job.Name} ({job.Type})");
        }

        deleteOptions.Add(Messages.ResourceManager.GetString("ConsoleMenuQuit"));
        string title = Messages.ResourceManager.GetString("DeleteJobPrompt");
        int selection = NavigateMenu(deleteOptions.ToArray(), title);

        if (selection == deleteOptions.Count - 1)
        {
            return false;
        }

        var jobToDelete = jobs[selection];
        Console.Clear();
        ShowHeader();

        bool success = _backupViewModel.DeleteJob(jobToDelete);

        if (success)
        {
            Console.ForegroundColor = ConsoleTheme.MainColor;
            Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobSuccess"));
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("DeleteJobFailed"));
        }
        Console.ResetColor();
        return success;
    }


    private void ExecuteJobs()
    {
        var jobsList = _backupViewModel.Jobs?.ToList();

        if (jobsList == null || jobsList.Count == 0)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }

        string[] options = new string[jobsList.Count];
        for (var i = 0; i < jobsList.Count; i++)
        {
            options[i] = $"{jobsList[i].Name} ({jobsList[i].Type})";
        }
        List<int> selectedIndices = NavigateMultiSelect(options);

        Console.Clear();
        ShowHeader();
        
        if (selectedIndices.Count == 0)
        {
            Console.ForegroundColor = ConsoleTheme.WarningColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
            Console.ResetColor();
            return;
        }

        var success = true;
        foreach (int index in selectedIndices)
        {
            var job = jobsList[index];
            success &= _backupViewModel.ExecuteJob(job, this);
        }

        if (success)
        {
            Console.ForegroundColor = ConsoleTheme.MainColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsSuccess")); 
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsFailed"));
        }
        Console.ResetColor();
        
    }

    private void ExecuteAllJobs()
    {
        Console.Clear();
        ShowHeader();
        if (_backupViewModel.Jobs != null && _backupViewModel.Jobs.Count == 0)
        {
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }

        var success = true;
        if (_backupViewModel.Jobs != null)
            foreach (var job in _backupViewModel.Jobs.ToList())
                success &= _backupViewModel.ExecuteJob(job, this);

        if (success)
        {
            Console.ForegroundColor = ConsoleTheme.MainColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsSuccess")); 
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsFailed"));
        }
        Console.ResetColor();
    }

    private void ChangeLanguage()
    {
        var availableLanguages = new[] 
        {
            (Name: Messages.ResourceManager.GetString("ChangeLanguageEnglish"), Code: "en-US"),
            (Name: Messages.ResourceManager.GetString("ChangeLanguageFrench"),  Code: "fr-FR")
        };
        string?[] options = availableLanguages.Select(l => l.Name).ToArray();
        string? title = Messages.ResourceManager.GetString("ChangeLanguageTitle");
        int selection = NavigateMenu(options, title);
        string selectedLanguage = availableLanguages[selection].Code;
        string currentLanguage = SettingsService.GetInstance.Settings.Language;
        
        if (selectedLanguage == currentLanguage)
        {
            Console.ForegroundColor = ConsoleTheme.WarningColor;
            Console.WriteLine();
            Console.WriteLine(Messages.ResourceManager.GetString("WarningLanguageActive"));
            Console.ResetColor();
        }
        else
        {
            SettingsService.GetInstance.SetLanguage(selectedLanguage);
            Console.ForegroundColor = ConsoleTheme.MainColor;
            Console.WriteLine();
            Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
            Console.ResetColor();
        }
    }

    private int NavigateMenu(string?[] options, string? question = null)
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
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine($"> {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"{options[i]}");
                }
            }

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow when selection < options.Length - 1:
                    selection++;
                    break;
                case ConsoleKey.UpArrow when selection > 0:
                    selection--;
                    break;
                case ConsoleKey.Enter:
                    Console.CursorVisible = true;
                    return selection;
            }
        }
    }

    private List<int> NavigateMultiSelect(string[] options, string? question = null)
    {
        int selection = 0;
        List<int> selectedIndexes = [];
        Console.CursorVisible = false;

        while (true)
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(question);
            Console.ForegroundColor = ConsoleTheme.InstructionColor;
            Console.WriteLine(Messages.ResourceManager.GetString("MultipleSelectionAdvice"));
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < options.Length; i++)
            {
                bool isChecked = selectedIndexes.Contains(i);
                string checkbox = isChecked ? "[X]" : "[ ]";

                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine($"> {checkbox} {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    if (isChecked) Console.ForegroundColor = ConsoleTheme.SecondaryColor;
                    Console.WriteLine($"   {checkbox} {options[i]}");
                }

                Console.ResetColor();
            }

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.DownArrow when selection < options.Length - 1:
                    selection++;
                    break;
                case ConsoleKey.UpArrow when selection > 0:
                    selection--;
                    break;
                case ConsoleKey.Spacebar when selectedIndexes.Contains(selection):
                    selectedIndexes.Remove(selection);
                    break;
                case ConsoleKey.Spacebar:
                    selectedIndexes.Add(selection);
                    break;
                case ConsoleKey.Enter:
                    Console.CursorVisible = true;
                    return selectedIndexes;
            }
        }
    }

    private void AddToPath()
    {
        var pathExe = Path.GetDirectoryName(Environment.ProcessPath);

        if (string.IsNullOrWhiteSpace(pathExe))
        {
            Console.WriteLine(Ressources.Errors.PathAddError);
            return;
        }
        
        var pathActuel = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(pathActuel))
        {
            Console.WriteLine(Ressources.Errors.PathAddError);
            return;
        }
        
        if (pathActuel.Contains(pathExe))
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("AlreadyInPath"));
            Console.ResetColor();  
            return;
        }
        
        string nouveauPath = pathActuel + ";" + pathExe;
        Environment.SetEnvironmentVariable("PATH", nouveauPath, EnvironmentVariableTarget.User);
    
        Console.ForegroundColor = ConsoleTheme.MainColor;
        Console.WriteLine(Messages.ResourceManager.GetString("AddToPathSucces"));
        Console.ForegroundColor = ConsoleTheme.InstructionColor;
        Console.WriteLine(Messages.ResourceManager.GetString("Restart"));
        Console.ResetColor();
    }
}