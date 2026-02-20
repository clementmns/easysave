using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace EasySave.Core.Utils;

public static class CryptoUtils
{
    private const string CRYPTO_EXE_NAME = @"CryptoSoft.exe";
    private const string DEFAULT_ALGORITHM = "xor";
    private const string ENCRYPT_EXTENSION = ".lock";

    // RSA key stored in %AppData%
    private static string KeyDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "ProSoft", "EasySave", "keys");

    private static string PrivateKeyPath => Path.Combine(KeyDirectory, "private_key.xml");
    private static string PublicKeyPath  => Path.Combine(KeyDirectory, "public_key.xml");

    private static void EnsureKeysExist()
    {
        if (File.Exists(PrivateKeyPath) && File.Exists(PublicKeyPath))
            return;

        Directory.CreateDirectory(KeyDirectory);

        using var rsa = RSA.Create(4096);

        // Write private key
        File.WriteAllText(PrivateKeyPath, rsa.ToXmlString(includePrivateParameters: true));
        // Write public key
        File.WriteAllText(PublicKeyPath,  rsa.ToXmlString(includePrivateParameters: false));

        // Restrict private key file: remove inheritance, grant FullControl only to the current user
        RestrictFileToCurrentUser(PrivateKeyPath);
    }
    
    private static void RestrictFileToCurrentUser(string filePath)
    {
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Secure with ACL for Windows
            SecureWindowsDirectory(filePath);
        }
        else
        {
            // Secure with POSIX for Unix
            SecureUnixDirectory(filePath);
        }
    }

    public static (bool, long) EncryptFile(string sourcePath, string destinationPath, string? sourceRoot = null, string algorithm = DEFAULT_ALGORITHM)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var relativePath = string.IsNullOrWhiteSpace(sourceRoot)
            ? Path.GetFileName(sourcePath)
            : Path.GetRelativePath(sourceRoot, sourcePath);
        
        if (relativePath.Contains(".."))
            return (false, 0);
        
        var destFile = destinationPath + ENCRYPT_EXTENSION;

        var result = ExecuteCryptoCommand(algorithm, "encrypt", sourcePath, destFile);
        
        stopwatch.Stop();

        var ms = stopwatch.ElapsedMilliseconds;
        return (result, ms);
    }

    public static bool DecryptFile(string sourcePath, string destinationPath, string algorithm = DEFAULT_ALGORITHM)
    {
        return ExecuteCryptoCommand(algorithm, "decrypt", sourcePath, destinationPath);
    }

    private static bool ExecuteCryptoCommand(string algorithm, string action, string sourcePath, string destinationPath)
    {
        try
        {
            EnsureKeysExist();

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = SignTimestamp(timestamp);
            
            var cryptoExePath = GetCryptoSoftPath();
            if (cryptoExePath == null)
                return false;

            var processInfo = new ProcessStartInfo
            {
                FileName = cryptoExePath,
                Arguments = $"{algorithm} {action} \"{sourcePath}\" \"{destinationPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            processInfo.EnvironmentVariables["EASYSAVE_TIMESTAMP"] = timestamp;
            processInfo.EnvironmentVariables["EASYSAVE_SIGNATURE"] = signature;
            processInfo.EnvironmentVariables["EASYSAVE_PUBLIC_KEY_PATH"] = PublicKeyPath;

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process is { HasExited: true, ExitCode: 0 };
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static string SignTimestamp(string timestamp)
    {
        var privateKeyXml = File.ReadAllText(PrivateKeyPath);

        using var rsa = RSA.Create();
        rsa.FromXmlString(privateKeyXml);

        var data = Encoding.UTF8.GetBytes(timestamp);
        
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

        return Convert.ToBase64String(signature);
    }

    private static string? GetCryptoSoftPath()
    {
        var assemblyBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var currentDir = new DirectoryInfo(assemblyBaseDir);
        DirectoryInfo? solutionRoot = null;
        
        for (var i = 0; i < 4 && currentDir?.Parent != null; i++)
        {
            currentDir = currentDir.Parent;
        }
        solutionRoot = currentDir;
        
        var searchPaths = new List<string>
        {
            Path.Combine(assemblyBaseDir, CRYPTO_EXE_NAME),
            Path.Combine(Environment.CurrentDirectory, CRYPTO_EXE_NAME)
        };

        if (solutionRoot is not { Exists: true }) return searchPaths.FirstOrDefault(File.Exists);
        searchPaths.Add(Path.Combine(solutionRoot.FullName, "CryptoSoft", "bin", "Debug", "net10.0", "win-x64", CRYPTO_EXE_NAME));
        searchPaths.Add(Path.Combine(solutionRoot.FullName, "CryptoSoft", "bin", "Release", "net10.0", "win-x64", CRYPTO_EXE_NAME));

        return searchPaths.FirstOrDefault(File.Exists);
    }
    
    private static void SecureWindowsDirectory(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        var security = new DirectorySecurity();

        // block & delete perms
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

        // recovers the user ID
        var currentUser = WindowsIdentity.GetCurrent().User;
        if (currentUser == null) throw new Exception("Utilisateur introuvable");

        // define the total control to the actual user
        security.AddAccessRule(new FileSystemAccessRule(
            currentUser,
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow));

        directoryInfo.SetAccessControl(security);
    }
    
    private static void SecureUnixDirectory(string path)
    {
        // 700 mod
        const UnixFileMode mod = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

        File.SetUnixFileMode(path, mod);
    }
}
