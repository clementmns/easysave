using EasySave.Core.Model;

namespace EasySave.CLI.Resources;

public sealed class AppProperties : IAppProperties
{
    public string AppSaveDirectory { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ProSoft/EasySave";

    public int MaxJobs { get; } = 5;

    public string? CryptoSoftPath { get; } = null;
}
