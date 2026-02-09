using EasySave.Model;
using EasySave.Service;
using EasySave.Ressources;
using EasySave.ViewModel;

namespace EasySave.View;

/// <summary>
/// Console view for managing backup jobs. Implement IProgressionObserver to be notified of backup progression.
/// </summary>
public class ConsoleAppView : IProgressionObserver
{
    private readonly BackupViewModel _backupViewModel;

    public ConsoleAppView(string appSaveDirectory)
    {
        _backupViewModel = new BackupViewModel(appSaveDirectory);
    }

    /// <summary>
    /// Called when backup progression changes.
    /// </summary>
    /// <param name="progression">percentage of progression</param>
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
    
    /// <summary>
    /// Execute jobs from command line arguments.
    /// </summary>
    /// <param name="args">1-3 for 1 to 3 or 1;3 for 1 and 3</param>
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

    /// <summary>
    /// Displays the application header with logo.
    /// </summary>
    private static void ShowHeader()
    {
        Console.ForegroundColor = ConsoleTheme.MainColor;
        var logo = AppLogo.Logo;
        foreach (var line in logo)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    /// <summary>
    /// Main loop of the console application, displaying the menu and handling user input.
    /// </summary>
    public void Run()
    {
        var exit = false;
        const int maxFiles = 5;

        while (!exit)
        {
            // menu of the application
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

            var choice = NavigateMenu(options);
            Console.Clear();
            ShowHeader();

            switch (choice)
            {
                case 0:
                    ViewJobs();
                    break;

                case 1:
                    // Add a new job
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
                    $@"{i + 1}. {job.Name} ({job.SourcePath} -> {job.DestinationPath}) - Type: {job.Type}");
            }
        }
    }

    private void AddJob()
    {
        // check all fields
        Console.WriteLine(Messages.ResourceManager.GetString("AddJobName"));
        var name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine(Messages.ResourceManager.GetString("AddJobSourcePath"));
        var sourcePath = Console.ReadLine() ?? string.Empty;

        Console.WriteLine(Messages.ResourceManager.GetString("AddJobDestinationPath"));
        var destinationPath = Console.ReadLine() ?? string.Empty;

        string?[] options =
        [
            Messages.ResourceManager.GetString("AddJobTypeDifferential"),
            Messages.ResourceManager.GetString("AddJobTypeFull")
        ];
        var question = Messages.ResourceManager.GetString("AddJobSaveType");
        var selection = NavigateMenu(options, question);
        var saveType = selection == 0 ? BackupType.Differential : BackupType.Full;

        BackupJob job;
        var currentJobs = _backupViewModel.Jobs?.ToList() ?? [];
        try
        {
            // create the job using the singleton factory
            job = BackupJobFactory.GetInstance().CreateJob(name, sourcePath, destinationPath, saveType, currentJobs);
        }
        catch
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
        var success = _backupViewModel.AddJob(job);
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

        var deleteOptions = new List<string?>();
        foreach (var job in jobs)
        {
            deleteOptions.Add($"{job.Name} ({job.Type})");
        }

        deleteOptions.Add(Messages.ResourceManager.GetString("ConsoleMenuQuit"));
        var title = Messages.ResourceManager.GetString("DeleteJobPrompt");
        var selection = NavigateMenu(deleteOptions.ToArray(), title);

        if (selection == deleteOptions.Count - 1)
        {
            return false;
        }

        var jobToDelete = jobs[selection];
        Console.Clear();
        ShowHeader();

        var success = _backupViewModel.DeleteJob(jobToDelete);

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

        var options = new string[jobsList.Count];
        for (var i = 0; i < jobsList.Count; i++)
        {
            options[i] = $"{jobsList[i].Name} ({jobsList[i].Type})";
        }
        var selectedIndices = NavigateMultiSelect(options);

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
        foreach (var index in selectedIndices)
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
        if (_backupViewModel.Jobs is { Count: 0 })
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
        var options = availableLanguages.Select(l => l.Name).ToArray();
        var title = Messages.ResourceManager.GetString("ChangeLanguageTitle");
        var selection = NavigateMenu(options, title);
        var selectedLanguage = availableLanguages[selection].Code;
        var currentLanguage = SettingsService.GetInstance.Settings.Language;
        
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

        var selection = 0;
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

            for (var i = 0; i < options.Length; i++)
            {
                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine(@$"> {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(@$"{options[i]}");
                }
            }

            // handle user input for navigation
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
        var selection = 0;
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

            for (var i = 0; i < options.Length; i++)
            {
                var isChecked = selectedIndexes.Contains(i);
                var checkbox = isChecked ? "[X]" : "[ ]";

                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine(@$"> {checkbox} {options[i]}");
                    Console.ResetColor();
                }
                else
                {
                    if (isChecked) Console.ForegroundColor = ConsoleTheme.SecondaryColor;
                    Console.WriteLine(@$"   {checkbox} {options[i]}");
                }

                Console.ResetColor();
            }

            var key = Console.ReadKey(true).Key;

            // handle user input for navigation
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

    /// <summary>
    /// Add the application directory to the user PATH environment variable.
    /// Allows to use the application from any terminal without specifying the full path. 
    /// </summary>
    private void AddToPath()
    {
        var pathExe = Path.GetDirectoryName(Environment.ProcessPath);

        if (string.IsNullOrWhiteSpace(pathExe))
        {
            Console.WriteLine(Errors.PathAddError);
            return;
        }
        
        var actualPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(actualPath))
        {
            Console.WriteLine(Errors.PathAddError);
            return;
        }
        
        if (actualPath.Contains(pathExe))
        {
            Console.ForegroundColor = ConsoleTheme.SecondaryColor;
            Console.WriteLine(Messages.ResourceManager.GetString("AlreadyInPath"));
            Console.ResetColor();  
            return;
        }
        
        var newPath = actualPath + ";" + pathExe;
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    
        Console.ForegroundColor = ConsoleTheme.MainColor;
        Console.WriteLine(Messages.ResourceManager.GetString("AddToPathSucces"));
        Console.ForegroundColor = ConsoleTheme.InstructionColor;
        Console.WriteLine(Messages.ResourceManager.GetString("Restart"));
        Console.ResetColor();
    }
}