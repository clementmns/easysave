namespace EasySave.Core.Model;

public interface IAppProperties
{
    string AppSaveDirectory { get; }
    int MaxJobs { get; }
    string? CryptoSoftPath { get; }
}