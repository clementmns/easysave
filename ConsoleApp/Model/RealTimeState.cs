namespace EasySave.ConsoleApp.Model;

public class RealTimeState
{
    public DateTime LastUpdate { get; set; }
    public bool IsActive { get; set; }
    public int TotalFiles { get; set; }
    public long FileSize { get; set; }
    public int Progression { get; set; }
    public int RemainingFiles { get; set; }
    public long RemainingFilesSize { get; set; }

    public override string ToString()
    {
        return $"RealTimeState(LastUpdate={LastUpdate}, IsActive={IsActive}, TotalFiles={TotalFiles}, FileSize={FileSize}, Progression={Progression}, RemainingFiles={RemainingFiles}, RemainingFilesSize={RemainingFilesSize})";
    }
}