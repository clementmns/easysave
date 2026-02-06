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

        // cli execution (`EasySave.exe 1-3` or `EasySave.exe 1-3`)
        if (args.Length > 0)
        {
            consoleAppView.RunWithArgs(args);
            return;
        }

        // console gui execution
        consoleAppView.Run();
    }
}
