using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EasySave.Core.Utils;

/// <summary>
/// Utility class for file operations.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Copy a file to a new location with progress reporting.
    /// </summary>
    public static (bool, long) CopyFile(string sourceFile, string destinationDir, string? sourceRoot, Action<long, long>? onProgress)
    {
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var relativePath = string.IsNullOrWhiteSpace(sourceRoot)
                ? Path.GetFileName(sourceFile)
                : Path.GetRelativePath(sourceRoot, sourceFile);
        
            if (relativePath.Contains(".."))
                return (false, 0);
        
            var destinationFileName = Path.Combine(destinationDir, relativePath);
            var destinationParent = Path.GetDirectoryName(destinationFileName);
            if (!string.IsNullOrWhiteSpace(destinationParent) && !Directory.Exists(destinationParent)) Directory.CreateDirectory(destinationParent);
            
            stopwatch.Stop();

            var ms = stopwatch.ElapsedMilliseconds;
            if (File.Exists(destinationFileName))
            {
                var attributes = File.GetAttributes(destinationFileName);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(destinationFileName, attributes & ~FileAttributes.ReadOnly);
                }
            }
            
            var fileSize = new FileInfo(sourceFile).Length;

            // size threshold for progress reporting (1 MB)
            if (onProgress != null && fileSize > 1024 * 1024)
            {
                const int bufferSize = 1024 * 1024;
                var buffer = new byte[bufferSize];
                
                using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                using var destinationStream = new FileStream(destinationFileName, FileMode.Create, FileAccess.Write);
                
                long totalBytesRead = 0;
                int bytesRead;
                
                while ((bytesRead = sourceStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    onProgress(totalBytesRead, fileSize);
                }
            }
            else
            {
                File.Copy(sourceFile, destinationFileName, true);
            }
            
            return (true, ms);
        }
        catch
        {
            return (false,0);
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
        catch
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (string.IsNullOrEmpty(root) || !root.Contains(':'))
                {
                    throw new ArgumentException("The path is not a valid absolute");
                }

                var driveLetter = root.Replace(":", "$").TrimEnd('\\');
                var pathWithoutRoot = fullPath.Substring(root.Length);
                var machineName = Environment.MachineName;

                return $@"\\{machineName}\{driveLetter}\{pathWithoutRoot}";
            }
            else
            {
                if (string.IsNullOrEmpty(root) || root != "/")
                {
                    throw new ArgumentException("The path is not a valid absolute Unix path");
                }

                var pathWithoutRoot = fullPath.TrimStart('/');
                var machineName = Environment.MachineName;

                return $@"//{machineName}/{pathWithoutRoot}";
            }
        }
        catch
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
        catch
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
            return Directory.Exists(path);
        }
        catch
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
            var fileSize =  fileInfo.Length; // In bytes
            return fileSize;
        }
        catch
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
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if a file has a priority extension.
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <param name="priorityExtensions">List of priority extensions</param>
    /// <returns></returns>
    private static bool HasPriorityExtension(string filePath, List<string> priorityExtensions)
    {
        try
        {
            var extension = Path.GetExtension(filePath);
            return priorityExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Separate files into priority and non-priority lists.
    /// </summary>
    /// <param name="files">List of files</param>
    /// <param name="priorityExtensions">List of priority extensions</param>
    /// <returns>Tuple of (priorityFiles, nonPriorityFiles)</returns>
    public static (List<FileInfo> priorityFiles, List<FileInfo> nonPriorityFiles) SeparatePriorityFiles(
        List<FileInfo> files,
        List<string> priorityExtensions)
    {
        var priorityFiles = new List<FileInfo>();
        var nonPriorityFiles = new List<FileInfo>();

        foreach (var file in files)
        {
            if (HasPriorityExtension(file.FullName, priorityExtensions)) priorityFiles.Add(file);
            else nonPriorityFiles.Add(file);
        }

        return (priorityFiles, nonPriorityFiles);
    }

    /// <summary>
    /// Check if a job has any priority files in its source path.
    /// </summary>
    /// <param name="sourcePath">Source path (file or directory)</param>
    /// <param name="priorityExtensions">List of priority extensions</param>
    /// <returns></returns>
    public static bool HasPriorityFiles(string sourcePath, List<string> priorityExtensions)
    {
        try
        {
            if (priorityExtensions.Count == 0) return false;

            if (File.Exists(sourcePath)) return HasPriorityExtension(sourcePath, priorityExtensions);

            if (!Directory.Exists(sourcePath)) return false;
            var files = GetAllFiles(sourcePath);
            return files.Any(f => HasPriorityExtension(f.FullName, priorityExtensions));
        }
        catch
        {
            return false;
        }
    }
}
