using System.Diagnostics;

namespace EasySave.Core.Utils;

public static class CryptoUtils
{
    private const string CRYPTO_EXE_NAME = @"C:/Users/timmf/Documents/GitHub/easysave/CryptoSoft/bin/Debug/net8.0/win-x64/CryptoSoft.exe";
    private const string DEFAULT_KEY = "EasySave2024!";
    private const string DEFAULT_ALGORITHM = "xor";

    public static bool EncryptFile(string sourcePath, string destinationPath, string key = DEFAULT_KEY, string algorithm = DEFAULT_ALGORITHM)
    {
        return ExecuteCryptoCommand(algorithm, "encrypt", sourcePath, destinationPath, key);
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
                Console.WriteLine($"[CRYPTO ERROR] {CRYPTO_EXE_NAME} not found");
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

            Console.WriteLine($"CryptoSoft error : {output} {error}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling CryptoSoft : {ex.Message}");
            return false;
        }
    }

    private static string? GetCryptoSoftPath()
    {
        // Looking for CryptoSoft.exe
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CRYPTO_EXE_NAME),
            Path.Combine(Environment.CurrentDirectory, CRYPTO_EXE_NAME),
            Path.Combine(Environment.CurrentDirectory, "CryptoSoft", "bin", "Release", "net8.0", "win-x64", CRYPTO_EXE_NAME)
        };

        return searchPaths.FirstOrDefault(File.Exists);
    }
}
