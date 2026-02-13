using System.Diagnostics;

namespace EasySave.Core.Utils;

public static class CryptoUtils
{
    private const string CRYPTO_EXE_NAME = @"C:/Users/timmf/Documents/GitHub/easysave/CryptoSoft/bin/Debug/net8.0/win-x64/CryptoSoft.exe";
    private const string DEFAULT_KEY = "dda272ea2cc4fe8774d834acc05f78149ab55ac0e32804377b1c06b1d4ba1e39"; // hash of "EasySaveCore"
    private const string DEFAULT_ALGORITHM = "xor";
    private const string ENCRYPT_EXTENSION = ".lock";

    public static (bool, long) EncryptFile(string sourcePath, string destinationPath, string? sourceRoot = null, string key = DEFAULT_KEY, string algorithm = DEFAULT_ALGORITHM)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var relativePath = string.IsNullOrWhiteSpace(sourceRoot)
            ? Path.GetFileName(sourcePath)
            : Path.GetRelativePath(sourceRoot, sourcePath);
        
        if (relativePath.Contains(".."))
            return (false, 0);
        
        var destFile = Path.Combine(destinationPath, relativePath) + ENCRYPT_EXTENSION;

        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
        var result = ExecuteCryptoCommand(algorithm, "encrypt", sourcePath, destFile, key);
        
        stopwatch.Stop();

        var ms = stopwatch.ElapsedMilliseconds;
        return (result, ms);
    }

    public static bool DecryptFile(string sourcePath, string destinationPath, string key = DEFAULT_KEY, string algorithm = DEFAULT_ALGORITHM)
    {
        return ExecuteCryptoCommand(algorithm, "decrypt", sourcePath, destinationPath, key);
    }

    private static bool ExecuteCryptoCommand(string algorithm, string action, string sourcePath, string destinationPath, string key)
    {
        try
        {
            var cryptoExePath = GetCryptoSoftPath();
            if (cryptoExePath == null)
            {
                return false;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = cryptoExePath,
                Arguments = $"{algorithm} {action} \"{sourcePath}\" \"{destinationPath}\" \"{key}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();

            if (process.ExitCode == 0 && output.StartsWith("SUCCESS"))
            {
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private static bool VerifyHash(string sourceHash)
    {
        // Check the hash between the process that launched Cryptosoft to ensure that only EasySave-Core can be called Cryptosoft.
        return true;
    }

    private static string? GetCryptoSoftPath()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CRYPTO_EXE_NAME),
            Path.Combine(Environment.CurrentDirectory, CRYPTO_EXE_NAME),
            Path.Combine(Environment.CurrentDirectory, "CryptoSoft", "bin", "Release", "net10.0", "win-x64", CRYPTO_EXE_NAME)
        };

        return searchPaths.FirstOrDefault(File.Exists);
    }
}
