using System.Diagnostics;
using System.Linq;

namespace EasySave.Core.Service;

public class ProcessMonitorService
{
    private static ProcessMonitorService? _instance;

    public static ProcessMonitorService Instance => _instance ??= new ProcessMonitorService();

    public bool IsBusinessSoftwareRunning
    {
        get
        {
            try
            {
                var processName = SettingsService.GetInstance.Settings.BusinessSoftwareProcessName;
                if (string.IsNullOrWhiteSpace(processName)) return false;
                
                var processes = Process.GetProcesses();
                var isRunning = processes.Any(p => 
                    p.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase));
                
                // Debug information
                if (isRunning)
                {
                    Console.WriteLine($"[ProcessMonitor] Business software '{processName}' detected!");
                }
                
                return isRunning;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessMonitor] Error checking business software: {ex.Message}");
                return false;
            }
        }
    }

    private ProcessMonitorService() { }
}
