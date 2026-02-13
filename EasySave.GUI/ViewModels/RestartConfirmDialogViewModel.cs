using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.ViewModels;

public partial class RestartConfirmDialogViewModel : DialogViewModel
{
    private readonly Window? _dialogWindow;

    public RestartConfirmDialogViewModel(Window? dialogWindow = null)
    {
        _dialogWindow = dialogWindow;
    }

    [RelayCommand]
    public void CancelCommand() => _dialogWindow?.Close();

    [RelayCommand]
    public void RestartCommand()
    {
        try
        {
            RestartApplication();
        }
        finally
        {
            ShutdownApplication();
        }
    }

    private static void RestartApplication()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath)) return;

        var args = Environment.GetCommandLineArgs().Skip(1);
        var argumentString = string.Join(' ', args.Select(EscapeArgument));

        var startInfo = new ProcessStartInfo
        {
            FileName = processPath,
            Arguments = argumentString,
            UseShellExecute = true
        };

        Process.Start(startInfo);
    }

    private static string EscapeArgument(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (!value.Contains(' ') && !value.Contains('"')) return value;
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    private static void ShutdownApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            return;
        }

        Environment.Exit(0);
    }
}
