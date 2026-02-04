using EasySave.ConsoleApp.Utils;
namespace EasySave.ConsoleApp.Model;

public class BackupJob
{
    public int _id { get; set; }
    public string _name { get; set; }
    public string _sourcePath { get; set; }
    public string _destinationPath { get; set; }
    public BackupType _type { get; set; }
    public bool? _isDir {get; set;}
    public RealTimeState _state { get; set; }

    public BackupJob(int id, string name, string sourcePath, string destinationPath, BackupType type)
    {
        this._id = id;
        this._name = name;
        this._sourcePath = sourcePath;
        this._destinationPath = destinationPath;
        this._type = type;
        this._state = new RealTimeState();
        this._isDir = FileUtils.IsDirectory(sourcePath); // verifier si null : on renvoie à l'utilisateur une erreur
    }
}