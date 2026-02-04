using EasyLog;
using EasyLog.Strategies;
using EasySave.ConsoleApp.Model;
using EasySave.ConsoleApp.Ressources;
using EasySave.ConsoleApp.Service;
using EasySave.ConsoleApp.ViewModel;

namespace EasySave.ConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        // init app settings and logger
        var appSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ProSoft/EasySave";
        Logger.Init(appSaveDirectory, [new JsonLoggerStrategy()]);
        SettingsService.Init(appSaveDirectory);
        
        Console.WriteLine(Messages.HelloWorld);
        
        // call view and view call BackupViewModel
        var viewModel = new BackupViewModel(appSaveDirectory);
        Console.WriteLine($"Number of backup jobs: {viewModel.Jobs.Count}");

        // viewModel.AddJob("test2", "test2", "test2", BackupType.Full);
        Console.WriteLine(viewModel.Jobs.Count);
        
        viewModel.ExecuteJob(viewModel.Jobs.First());
        Console.WriteLine(viewModel.Jobs.First().ToString());
    }
}