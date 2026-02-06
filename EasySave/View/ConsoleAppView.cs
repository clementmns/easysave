using EasySave.Model;
using EasySave.Service;
using EasySave.Ressources;
using EasySave.ViewModel;

namespace EasySave.View;

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
        Console.ForegroundColor = ConsoleTheme.MainColor;
        string[] logo = AppLogo.Logo;
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
                    var currentJobs = _backupViewModel.Jobs?.ToList() ?? [];
                    if (currentJobs.Count == maxFiles)
                    {
                        Console.ForegroundColor = ConsoleTheme.WarningColor;
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

        string[] options =
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
            Console.ForegroundColor = ConsoleTheme.ErrorColor;
            Console.WriteLine(Messages.ResourceManager.GetString("AddJobFailed"));
            Console.ResetColor();
            return;
        }

        Console.Clear();
        Console.WriteLine(_backupViewModel.AddJob(job)
            ? Messages.ResourceManager.GetString("AddJobSuccess")
            : Messages.ResourceManager.GetString("AddJobFailed"));
    }

    private void DeleteJob() 
    {
        var jobs = _backupViewModel.Jobs?.ToList();

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

        deleteOptions.Add(Messages.ResourceManager.GetString("ConsoleMenuQuit"));
        string title = Messages.ResourceManager.GetString("DeleteJobPrompt");
        int selection = NavigateMenu(deleteOptions.ToArray(), title);

        if (selection == deleteOptions.Count - 1)
        {
            return;
        }

        var jobToDelete = jobs[selection];
        Console.Clear();

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
    }


    private void ExecuteJobs()
    {
        var jobsList = _backupViewModel.Jobs?.ToList();

        if (jobsList == null || jobsList.Count == 0)
        {
            Console.Clear();
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoJobs"));
            return;
        }

        string[] options = new string[jobsList.Count];
        for (int i = 0; i < jobsList.Count; i++)
        {
            options[i] = $"{jobsList[i].Name} ({jobsList[i].Type})";
        }
        List<int> selectedIndices = NavigateMultiSelect(options);

        Console.Clear();
        
        if (selectedIndices.Count == 0)
        {
            Console.ForegroundColor = ConsoleTheme.WarningColor;
            Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsNoValid"));
            Console.ResetColor();
            return;
        }
        
        foreach (int index in selectedIndices)
        {
            var job = jobsList[index];
            _backupViewModel.ExecuteJob(job);
        }

        Console.ForegroundColor = ConsoleTheme.MainColor;
        Console.WriteLine(Messages.ResourceManager.GetString("ExecuteJobsSuccess"));
        Console.ResetColor();
        
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
        var availableLanguages = new[] 
        {
            (Name: Messages.ResourceManager.GetString("ChangeLanguageEnglish"), Code: "en-US"),
            (Name: Messages.ResourceManager.GetString("ChangeLanguageFrench"),  Code: "fr-FR")
        };
        string[] options = availableLanguages.Select(l => l.Name).ToArray();
        string title = Messages.ResourceManager.GetString("ChangeLanguageTitle");
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
            Console.WriteLine(Messages.ResourceManager.GetString("ChangeLanguageSuccess"));
        }
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

    private List<int> NavigateMultiSelect(string[] options, string? question = null)
    {
        int selection = 0;
        List<int> selectedIndexes = new List<int>();
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
                    Console.ResetColor();
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
                else if (key == ConsoleKey.Spacebar)
                {
                    if (selectedIndexes.Contains(selection))
                    {
                        selectedIndexes.Remove(selection);
                    }
                    else
                    {
                        selectedIndexes.Add(selection);
                    }
                }
                else if (key == ConsoleKey.Enter)
                {
                    Console.CursorVisible = true;
                    return selectedIndexes;
                }
            }
        }
    }
