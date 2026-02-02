namespace EasySave.ConsoleApp.Model;

public class BackupJob
{
    private int _id { get; set; }
    private string _name { get; set; }
    private string _sourcePath { get; set; }
    private string _destinationPath { get; set; }
    private BackupType _type { get; set; }
    private RealTimeState _state { get; set; }

    public BackupJob(int id, string name, string sourcePath, string destinationPath, BackupType type)
    {
        this._id = id;
        this._name = name;
        this._sourcePath = sourcePath;
        this._destinationPath = destinationPath;
        this._type = type;
        this._state = new RealTimeState();
    }
}