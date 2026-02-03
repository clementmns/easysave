using System;
using System.Collections.Generic;
using System.Linq;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.ViewModel;

namespace EasySave.ConsoleApp.View;

public class ConsoleAppView
{
    private readonly BackupViewModel _viewModel;

    public ConsoleAppView(BackupViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void ShowHeader()
    {
        Console.WriteLine("===================================");
        Console.WriteLine("        Easy Save Application      ");
        Console.WriteLine("===================================");
    }

    public void Run()
    {
        bool exit = false;

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
        var jobs = _viewModel.GetJobs();
        Console.WriteLine("Here the jobs : ");
        
        if (jobs == null || jobs.Count == 0)
        {
            Console.WriteLine("No job available.");
        }
        else
        {
            foreach (var job in jobs)
            {
                Console.WriteLine($"- {job.name} ({job.sourcePath} -> {job.destinationPath}) - Type: {job.type}");
            }
        }Console.ReadLine();
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
        var job = new BackupJob(name,sourcePath, destinationPath, backupType);

        if (_viewModel.AddJob(job))
        {
            Console.WriteLine("Job add with success.");
        }
        else
        {
            Console.WriteLine("Job add failed.");
        }
        Console.ReadLine();
    }

    private void DeleteJob()
    {
        Console.Write("Enter the job to remove : ");
        var name = Console.ReadLine() ?? string.Empty;
        var jobs = _viewModel.GetJobs();
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
        var jobs = _viewModel.GetJobs()?.FindAll(j => names?.Contains(j.Name) == true);

        if (jobs != null && jobs.Count > 0)
        {
            _viewModel.RunJobs(jobs);
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
        _viewModel.RunAllJobs();
        Console.WriteLine("All jobs have been successfully completed.");
        Console.ReadLine();
    }

    private void ChangeLanguage()
    {
        Console.Write("Change the language (EN/FR) : ");
        var langInput = Console.ReadLine() ?? string.Empty;
        
        if (Enum.TryParse<Language>(langInput, true, out var language))
        {
            _viewModel.Settings.Language = language;
            Console.WriteLine("Language modified.");
        }
        else
        {
            Console.WriteLine("Invalid language.");
        }
        Console.ReadLine();
    }
}
