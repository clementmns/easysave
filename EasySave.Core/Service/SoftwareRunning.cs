using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Core.Service;

public class ProcessMonitorService : INotifyPropertyChanged
{
    private static ProcessMonitorService? _instance;
    private readonly Timer _monitorTimer;
    private bool _isBusinessSoftwareRunning;
    private readonly string[] _businessProcessNames = { "calc", "calculator", "Calculator" };

    public static ProcessMonitorService Instance => _instance ??= new ProcessMonitorService();

    public bool IsBusinessSoftwareRunning
    {
        get => _isBusinessSoftwareRunning;
        private set
        {
            if (_isBusinessSoftwareRunning != value)
            {
                _isBusinessSoftwareRunning = value;
                OnPropertyChanged();
            }
        }
    }

    private ProcessMonitorService()
    {
        _monitorTimer = new Timer(CheckBusinessSoftware, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    private void CheckBusinessSoftware(object? state)
    {
        try
        {
            var processes = Process.GetProcesses();
            var isRunning = processes.Any(p => 
                _businessProcessNames.Any(name => 
                    p.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase)));
            
            IsBusinessSoftwareRunning = isRunning;
        }
        catch (Exception)
        {
            IsBusinessSoftwareRunning = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _monitorTimer?.Dispose();
    }
}
