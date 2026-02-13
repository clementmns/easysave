using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.GUI.ViewModels;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isVisible;
    
    protected TaskCompletionSource CloseTask = new();

    public async Task WaitAsync()
    {
        await CloseTask.Task;
    }

    public void Show()
    {
        if (CloseTask.Task.IsCompleted) CloseTask = new TaskCompletionSource();
        IsVisible = true;
    }

    public void Close()
    {
        IsVisible = false;
        CloseTask.SetResult();
    }
}