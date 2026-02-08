using System.Diagnostics.Tracing;
using EasySave.Model;
using EasySave.Service;
using EasySave.Ressources;
using EasySave.ViewModel;
using static EasySave.Service.SettingsService;

namespace EasySave.View;

// TODO : changer les ressources françaises -> travaux de sauvegarde
// TODO : changer le full lorsqu'on affiche les travaux en fonction de la langue


/// <summary>
/// Console view for managing backup jobs. Implement IProgressionObserver to be notified of backup progression.
/// </summary>
public class ConsoleAppView : IProgressionObserver
{
    private readonly BackupViewModel _backupViewModel;
    private int _consoleWidth;
    private int _consoleHeight;
    private const int _maxContentWidth = 120;
    private int _contentPadding;
    private readonly string _version = GetInstance.Settings.Version ;

    public ConsoleAppView(string appSaveDirectory)
    {
        _backupViewModel = new BackupViewModel(appSaveDirectory);
        _consoleWidth = Console.WindowWidth;
        _consoleHeight = Console.WindowHeight;
        _contentPadding = Math.Min(60, _consoleWidth - 4);
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
            Console.CursorVisible = false;
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleTheme.InstructionColor;
            WriteCentered(Messages.ResourceManager.GetString("PressKeyToContinue"));
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();
        }
    }
    /// <summary>
    /// Calcul the left padding for center
    /// </summary>
    /// <param name="contentWidth"></param>
    /// <returns></returns>
    private int GetLeftPadding(int contentWidth)
    {
        return Math.Max(0, (_consoleWidth - contentWidth) / 2);
    }

    private int GetTopPadding(int contentHeight)
    {
        return Math.Max(0, (_consoleHeight - contentHeight) / 2);
    }

    /// <summary>
    /// Write centered line in console
    /// </summary>
    /// <param name="text"></param>
    private void WriteCentered(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        var padding = GetLeftPadding(text.Length);
        Console.WriteLine(new string(' ', padding) + text);
    }

    private void WriteSeparator(int width = 60)
    {
        var effectiveWidth = Math.Min(width, _consoleWidth - 4);
        var separator = new string('─', effectiveWidth);
        WriteCentered(separator);
    }
    
    /// <summary>
    /// Displays the application header with logo.
    /// </summary>
    private void ShowHeader()
    {
        Console.ForegroundColor = ConsoleTheme.MainColor;
        var logo = AppLogo.Logo;
        
        Console.WriteLine();
        Console.WriteLine();
        
        // write the logo (ascii art)
        foreach (var line in logo)
        {
            WriteCentered(line);
        }
        Console.WriteLine();
        
        // write version
        Console.ForegroundColor = ConsoleTheme.SecondaryColor;
        WriteCentered("v" + _version);
        Console.ResetColor();
        Console.WriteLine();
        
        WriteSeparator();
        Console.WriteLine();
        Console.WriteLine();
    }
    
    private int NavigateMenu(string?[] options, string? question = null)
    {

        var selection = 0;
        Console.CursorVisible = false;

        while (true)
        {
            Console.Clear();
            
            // TODO: calculer dynamiquement la largeur & hauteur pour ajuster
            
            ShowHeader();

            if (!string.IsNullOrWhiteSpace(question))
            {
                Console.ForegroundColor = ConsoleTheme.InstructionColor;
                WriteCentered(question);
                Console.ResetColor();
                Console.WriteLine();
            }
            
            var separatorWidth = Math.Min(60, _consoleWidth - 4);
            var contentPadding = GetLeftPadding(separatorWidth);

            for (var i = 0; i < options.Length; i++)
            {
                var prefix = i == selection ? "> " : "  ";
                var line = prefix + options[i];
                
                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine(new string(' ', contentPadding) + line);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(new string(' ', contentPadding) + line);
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
            
            
            Console.ForegroundColor = ConsoleTheme.InstructionColor;
            WriteCentered(question);
            Console.WriteLine();
            Console.WriteLine();
            WriteCentered(Messages.ResourceManager.GetString("MultipleSelectionAdvice"));
            Console.ResetColor();
            Console.WriteLine();
            
            var separatorWidth = Math.Min(60, _consoleWidth - 4);
            var leftPadding = GetLeftPadding(separatorWidth);

            for (var i = 0; i < options.Length; i++)
            {
                var isChecked = selectedIndexes.Contains(i);
                var checkbox = isChecked ? "[X]" : "[ ]";
                var prefix = i == selection ? "> " : "  ";
                var line = $"{prefix}{checkbox} {options[i]}";

                if (i == selection)
                {
                    Console.ForegroundColor = ConsoleTheme.MainColor;
                    Console.WriteLine(new string(' ', leftPadding) + line);
                    Console.ResetColor();
                }
                else
                {
                    if (isChecked) Console.ForegroundColor = ConsoleTheme.SecondaryColor;
                    Console.WriteLine(new string(' ', leftPadding) + line);
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
                case ConsoleKey.Spacebar:
                    if (!selectedIndexes.Remove(selection))
                    {
                        selectedIndexes.Add(selection);
                    }
                    break;
                case ConsoleKey.Enter:
                    return selectedIndexes;
            }
        }
    }

    private void ViewJobs()
    {
        var jobs = _backupViewModel.Jobs?.ToList();
        if (jobs == null || jobs.Count == 0)
        {
            WriteCentered(Messages.ResourceManager.GetString("ViewJobsNoJob"));
            return;
        }
        
        var separatorWidth = Math.Min(60, _consoleWidth - 4);
        var leftPadding = GetLeftPadding(separatorWidth);
        
        Console.WriteLine();
        WriteCentered(Messages.ResourceManager.GetString("ViewJobsTitle"));
        Console.WriteLine();
        Console.WriteLine();
        
        foreach (var job in jobs)
        {
            var jobInfo = $"{job.Name} - {job.Type}";
            Console.WriteLine(new string(' ', leftPadding) + jobInfo);
            
            Console.ForegroundColor = ConsoleTheme.SecondaryColor;
            var sourcePath = $"  source: {job.SourcePath}"; // TODO: mettre dans les ressources
            var destPath = $"  destination: {job.DestinationPath}"; // TODO: mettre dans les ressources
            
            Console.WriteLine(new string(' ', leftPadding) + sourcePath);
            Console.WriteLine(new string(' ', leftPadding) + destPath);
            
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    // TODO: faire cette méthode
    private void AddJob()
    {
        Console.ForegroundColor = ConsoleTheme.InstructionColor;
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
            WriteCentered(Messages.ResourceManager.GetString("ViewJobsNoJob"));
            return false;
        }

        // TODO : ajouter des infos sur les jobs pour supprimer (source / dest path) 
        var deleteOptions = new List<string>();
        foreach (var job in jobs)
        {
            deleteOptions.Add($"{job.Name} ({job.Type})");
        }

        deleteOptions.Add(Messages.ResourceManager.GetString("ConsoleMenuQuit")!);
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
            Console.ForegroundColor = ConsoleTheme.SecondaryColor;
            WriteCentered(Messages.ResourceManager.GetString("DeleteJobSuccess"));
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Messages.ResourceManager.GetString("DeleteJobFailed"));
        }
        Console.ResetColor();
        return success;
    }

    // TODO: ajouter l'option quitter
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
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
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
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsSuccess")); 
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsFailed"));
        }
        Console.ResetColor();
        
    }

    private void ExecuteAllJobs()
    {
        Console.Clear();
        ShowHeader();
        if (_backupViewModel.Jobs is { Count: 0 })
        {
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }

        var success = true;
        if (_backupViewModel.Jobs != null)
            foreach (var job in _backupViewModel.Jobs.ToList())
                success &= _backupViewModel.ExecuteJob(job, this);

        if (success)
        {
            Console.ForegroundColor = ConsoleTheme.SecondaryColor;
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsSuccess")); 
        }
        else
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Messages.ResourceManager.GetString("ExecuteJobsFailed"));
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
        var currentLanguage = GetInstance.Settings.Language;
        
        if (selectedLanguage == currentLanguage)
        {
            Console.ForegroundColor = ConsoleTheme.WarningColor;
            Console.WriteLine();
            WriteCentered(Messages.ResourceManager.GetString("WarningLanguageActive"));
        }
        else
        {
            GetInstance.SetLanguage(selectedLanguage);
            Console.ForegroundColor = ConsoleTheme.MainColor;
            Console.WriteLine();
            WriteCentered(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
        }

        Console.ResetColor();
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
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Errors.PathAddError);
            Console.ResetColor();
            return;
        }
        
        var actualPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(actualPath))
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Errors.PathAddError);
            Console.ResetColor();
            return;
        }
        
        if (actualPath.Contains(pathExe))
        {
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            WriteCentered(Messages.ResourceManager.GetString("AlreadyInPath"));
            Console.ResetColor();  
            return;
        }
        
        var newPath = actualPath + ";" + pathExe;
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    
        Console.ForegroundColor = ConsoleTheme.SecondaryColor;
        WriteCentered(Messages.ResourceManager.GetString("AddToPathSucces"));
        Console.WriteLine();
        Console.ForegroundColor = ConsoleTheme.InstructionColor;
        WriteCentered(Messages.ResourceManager.GetString("Restart"));
        Console.ResetColor();
    }
    
    // TODO : faire que les barres de progressions se superposent 
    /// <summary>
    /// Called when backup progression changes.
    /// </summary>
    /// <param name="progression">percentage of progression</param>
    public void OnProgressionUpdated(int progression)
    {
        Console.Clear();
        ShowHeader();
        WriteCentered(@"Sauvegarde en cours...");
        Console.WriteLine();
        
        var barLength = Math.Min(100, Math.Max(40, _consoleWidth - 20));
        var filledLength = (int)((progression / 100.0) * barLength);
        var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
        
        Console.ForegroundColor = ConsoleTheme.MainColor;
        var progressBar = $"[{bar}] {progression}%";
        WriteCentered(progressBar);
        Console.WriteLine();
        Console.ResetColor();
    }
}