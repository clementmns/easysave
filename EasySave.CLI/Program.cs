using EasyLog;
using EasyLog.Strategies;
using EasySave.CLI.View;
using EasySave.Core.Service;

namespace EasySave.CLI;

internal abstract class Program
{
    private static void Main(string[] args)
    {
        // init app settings and logger
        var appSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ProSoft/EasySave.Core";
        Logger.Init(appSaveDirectory, [new JsonLoggerStrategy()]);
        SettingsService.Init(appSaveDirectory);

        var consoleAppView = new ConsoleAppView(appSaveDirectory);

        // cli execution (`EasySave.Core.exe 1-3` or `EasySave.Core.exe 1-3`)
        if (args.Length > 0)
        {
            consoleAppView.RunWithArgs(args);
            return;
        }

        // console gui execution
        consoleAppView.Run();
    }
}