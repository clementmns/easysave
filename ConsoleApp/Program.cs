using EasyLog;
using EasyLog.Strategies;
using EasySave.ConsoleApp.Service;
using EasySave.ConsoleApp.View;

namespace EasySave.ConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        // init app settings and logger
        var appSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ProSoft/EasySave";
        Logger.Init(appSaveDirectory, [new JsonLoggerStrategy()]);
        SettingsService.Init(appSaveDirectory);

        var consoleAppView = new ConsoleAppView(appSaveDirectory);
        consoleAppView.Run();
    }
}