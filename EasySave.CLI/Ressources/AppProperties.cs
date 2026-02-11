using System;
using EasySave.Core.Model;

namespace EasySave.CLI.Ressources;

public sealed class AppProperties : IAppProperties
{
    public string AppSaveDirectory { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ProSoft/EasySave";

    public int MaxJobs { get; } = 5;
}
