using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.ConsoleApp.Model;

public class RealTimeState : INotifyPropertyChanged
{
    public DateTime LastUpdate
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsActive
    {
        get;
        set => SetField(ref field, value);
    }

    public int TotalFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public long FileSize
    {
        get;
        set => SetField(ref field, value);
    }

    public int Progression
    {
        get;
        set => SetField(ref field, value);
    }

    public int RemainingFiles
    {
        get;
        set => SetField(ref field, value);
    }

    public long RemainingFilesSize
    {
        get;
        set => SetField(ref field, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString()
    {
        return $"RealTimeState(LastUpdate={LastUpdate}, IsActive={IsActive}, TotalFiles={TotalFiles}, FileSize={FileSize}, Progression={Progression}, RemainingFiles={RemainingFiles}, RemainingFilesSize={RemainingFilesSize})";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
