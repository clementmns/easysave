using EasyLog;
using EasyLog.Strategies;
using EasySave.ConsoleApp.Ressources;
using EasySave.ConsoleApp.Service;

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
    }
}