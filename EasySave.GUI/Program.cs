using Avalonia;
using System;
using EasySave.Core.Service;
using EasySave.GUI.Resources;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SettingsService.Init(new AppProperties());
        
        if (args.Length > 0)
        {
            var vm = new MainViewModel();
            var executed = vm.ExecuteJobsFromArgs(args[0]).GetAwaiter().GetResult();
            foreach (var (requestedIndex, result) in executed)
            {
                Console.WriteLine(!result
                    ? requestedIndex + " :" + Messages.ResourceManager.GetString("ExecuteJobsFailed")
                    : requestedIndex + " :" + Messages.ResourceManager.GetString("ExecuteJobsSuccess"));
            }
            return;
        }
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
