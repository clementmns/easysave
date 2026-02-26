using System.Diagnostics;

namespace EasySave.Core.Service;

public static class ProcessMonitorService
{
    public static bool IsBusinessSoftwareRunning
    {
        get
        {
            try
            {
                var processNames = SettingsService.GetInstance.Settings.BusinessSoftwareProcessName;
                if (string.IsNullOrWhiteSpace(processNames)) return false;

                var names = processNames.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (names.Length == 0) return false;
                
                var processes = Process.GetProcesses();
                var isRunning = processes.Any(p => names.Any(n => p.ProcessName.Contains(n, StringComparison.OrdinalIgnoreCase)));
                
                return isRunning;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
