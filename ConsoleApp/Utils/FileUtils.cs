using System.Security.Cryptography;
using EasyLog;

namespace EasySave.ConsoleApp.Utils;

public class FileUtils
{
    public static bool CopyFile(string sourceFile, string destinationDir) // path/to/text.txt -> path/to/dir 
    {
        try
        {
            string fileName = Path.GetFileName(sourceFile);
            string destinationFileName = Path.Combine(destinationDir, fileName, "_copy");
            
            // Use the Path.Combine method to safely append the file name to the path.
            File.Copy(sourceFile, destinationFileName, true); // true if the destination file should be replaced if it already exists; otherwise, false
            return true;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return false;
        }
    }

    public static List<FileInfo> GetAllFiles(string directoryPath)
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            List<FileInfo> filesInDir = dirInfo.GetFiles("*", SearchOption.AllDirectories).ToList();
            return filesInDir;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return new List<FileInfo>();
        }
        
    }

    public static string? ConvertToUnc(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);

            if (new Uri(fullPath).IsUnc)
            {
                return fullPath;
            }

            string? root = Path.GetPathRoot(fullPath);

            // Si la racine n'est pas un lecteur (pas de ":"), on ne peut pas convertir simplement
            if (string.IsNullOrEmpty(root) || !root.Contains(":"))
            {
                throw new ArgumentException("The path is not a valid absolute");
            }

            string driveLetter = root.Replace(":", "$").TrimEnd('\\');
            string pathWithoutRoot = fullPath.Substring(root.Length); // Récupérer le reste du chemin (sans la racine)
            string machineName = Environment.MachineName; // Combiner avec le nom de la machine

            return $@"\\{machineName}\{driveLetter}\{pathWithoutRoot}";
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return null;
        }
        
    }

    public static DateTime? GetLastModifiedDate(string filePath)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            DateTime lastModifiedDate = fileInfo.LastWriteTime;
            return lastModifiedDate;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return null;
        }
        
    }

    public static bool CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);

            // Checking Directory is created
            // Successfully or not
            if (!Directory.Exists(path))
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return false;
        }
        
    }

    public static long GetFileSize(string path)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
            long fileSize =  fileInfo.Length; // en bits
            return fileSize;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return 0;
        }
    }
    
    public static bool? IsDirectory(string path)
    {
        try
        {
            FileAttributes attrs = File.GetAttributes(path);
            return (attrs & FileAttributes.Directory) == FileAttributes.Directory;
        }
        catch (Exception e)
        {
            Logger.Instance.Write(e.ToString());
            return null;
        }
    }
}