namespace EasySave.Utils;

/// <summary>
/// Utility class for file operations.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Copy a file to a new location.
    /// </summary>
    /// <param name="sourceFile">SourceFile</param>
    /// <param name="destinationDir">Destination Directory</param>
    /// <param name="sourceRoot">Source root</param>
    /// <returns></returns>
    public static bool CopyFile(string sourceFile, string destinationDir, string? sourceRoot = null) // path/to/text.txt -> path/to/dir 
    {
        try
        {
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot)
                ? Path.GetFileName(sourceFile)
                : Path.GetRelativePath(sourceRoot, sourceFile);
            var destinationFileName = Path.Combine(destinationDir, relativePath);

            var destinationParent = Path.GetDirectoryName(destinationFileName);
            if (!string.IsNullOrWhiteSpace(destinationParent) && !Directory.Exists(destinationParent))
            {
                Directory.CreateDirectory(destinationParent);
            }
            
            // Use the Path.Combine method to safely append the file name to the path.
            File.Copy(sourceFile, destinationFileName, true); // true if the destination file should be replaced if it already exists; otherwise, false
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Get all files in a directory and its subdirectories.
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <returns></returns>
    public static List<FileInfo> GetAllFiles(string directoryPath)
    {
        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            var filesInDir = dirInfo.GetFiles("*", SearchOption.AllDirectories).ToList();
            return filesInDir;
        }
        catch (Exception e)
        {
            return [];
        }
        
    }

    /// <summary>
    /// Convert a path to a UNC path.
    /// </summary>
    /// <param name="path">relative path</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Can't be converted</exception>
    public static string? ConvertToUnc(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);

            if (new Uri(fullPath).IsUnc)
            {
                return fullPath;
            }

            var root = Path.GetPathRoot(fullPath);

            // Si la racine n'est pas un lecteur (pas de ":"), on ne peut pas convertir simplement
            if (string.IsNullOrEmpty(root) || !root.Contains(":"))
            {
                throw new ArgumentException("The path is not a valid absolute");
            }

            var driveLetter = root.Replace(":", "$").TrimEnd('\\');
            var pathWithoutRoot = fullPath.Substring(root.Length); // Récupérer le reste du chemin (sans la racine)
            var machineName = Environment.MachineName; // Combiner avec le nom de la machine

            return $@"\\{machineName}\{driveLetter}\{pathWithoutRoot}";
        }
        catch (Exception e)
        {
            return null;
        }
        
    }

    /// <summary>
    /// Get the last modified date of a file.
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns></returns>
    public static DateTime? GetLastModifiedDate(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var lastModifiedDate = fileInfo.LastWriteTime;
            return lastModifiedDate;
        }
        catch (Exception e)
        {
            return null;
        }
        
    }

    /// <summary>
    /// Create a directory if it doesn't exist.'
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns></returns>
    public static bool CreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);

            // Checking Directory is created
            // Successfully or not
            return Directory.Exists(path);
        }
        catch (Exception e)
        {
            return false;
        }
        
    }

    /// <summary>
    /// Get the size of a file in bytes.
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns></returns>
    public static long GetFileSize(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            var fileSize =  fileInfo.Length; // en bits
            return fileSize;
        }
        catch (Exception e)
        {
            return 0;
        }
    }
    
    /// <summary>
    /// Check if a path is a directory.
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns></returns>
    public static bool? IsDirectory(string path)
    {
        try
        {
            var attrs = File.GetAttributes(path);
            return (attrs & FileAttributes.Directory) == FileAttributes.Directory;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
