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
                var processName = SettingsService.GetInstance.Settings.BusinessSoftwareProcessName;
                if (string.IsNullOrWhiteSpace(processName)) return false;
                
                var processes = Process.GetProcesses();
                var isRunning = processes.Any(p => p.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase));
                
                return isRunning;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
