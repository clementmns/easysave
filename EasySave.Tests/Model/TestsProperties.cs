using EasySave.Core.Model;

namespace EasySave.Tests.Model;

public class TestsProperties : IAppProperties
{
    public string AppSaveDirectory { get; } = "";
    public int MaxJobs { get; } = 5;
}