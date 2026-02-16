using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Core.Model;

/// <summary>
/// Backup job model.
/// </summary>
public class BackupJob : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _sourcePath = string.Empty;
    private string _destinationPath = string.Empty;
    private BackupType _type;
    private RealTimeState _state = new();

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set => SetField(ref _sourcePath, value);
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set => SetField(ref _destinationPath, value);
    }

    public BackupType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    public RealTimeState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public BackupJob(int id, string name, string sourcePath, string destinationPath, BackupType type)
    {
        _id = id;
        _name = name;
        _sourcePath = sourcePath;
        _destinationPath = destinationPath;
        _type = type;
        _state = new RealTimeState();
    }

    public override string ToString()
    {
        return $"BackupJob(Id={Id}, Name={Name}, SourcePath={SourcePath}, DestinationPath={DestinationPath}, Type={Type}, State={State})";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        OnPropertyChanged(propertyName);
    }
}
