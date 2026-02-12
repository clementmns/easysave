using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft;

public enum CryptoAlgorithm { Xor, Aes }

public class FileManager
{
    private string SourcePath { get; }
    private string DestinationPath { get; }
    private string Key { get; }
    private CryptoAlgorithm Algorithm { get; }
    private bool IsEncryption { get; }

    public FileManager(string sourcePath, string destinationPath, string key, CryptoAlgorithm algorithm, bool isEncryption)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        Key = key;
        Algorithm = algorithm;
        IsEncryption = isEncryption;
    }

    /// <summary>
    /// Check if the file exists
    /// </summary>
    private bool CheckFile()
    {
        if (File.Exists(SourcePath))
            return true;

        Console.WriteLine("ERROR: Source file not found");
        return false;
    }

    /// <summary>
    /// Encrypts or decrypts the file based on the chosen algorithm
    /// </summary>
    public int TransformFile()
    {
        if (string.IsNullOrEmpty(Key))
        {
            Console.WriteLine("ERROR: The key cannot be empty.");
            return -1;
        }

        if (!CheckFile()) return -1;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var fileBytes = File.ReadAllBytes(SourcePath);
            var keyBytes = ConvertToByte(Key);

            var result = Algorithm switch
            {
                CryptoAlgorithm.Xor => XorMethod(fileBytes, keyBytes),
                CryptoAlgorithm.Aes => IsEncryption 
                    ? AesEncrypt(fileBytes, keyBytes) 
                    : AesDecrypt(fileBytes, keyBytes),
                _ => throw new NotImplementedException()
            };

            var destDir = Path.GetDirectoryName(DestinationPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.WriteAllBytes(DestinationPath, result);
            stopwatch.Stop();

            return (int)stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR:{ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Convert a string in byte array
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static byte[] ConvertToByte(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    /// <summary>
    /// </summary>
    /// <param name="fileBytes">Bytes of the file to convert</param>
    /// <param name="keyBytes">Key to use</param>
    /// <returns>Bytes of the encrypted file</returns>
    private static byte[] XorMethod(IReadOnlyList<byte> fileBytes, IReadOnlyList<byte> keyBytes)
    {
        var result = new byte[fileBytes.Count];
        for (var i = 0; i < fileBytes.Count; i++)
        {
            result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Count]);
        }

        return result;
    }
    
    private static byte[] AesEncrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey(key, 32);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
        
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        
        return result;
    }

    private static byte[] AesDecrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey(key, 32);
        
        var iv = new byte[16];
        Buffer.BlockCopy(data, 0, iv, 0, 16);
        aes.IV = iv;
        
        var encryptedData = new byte[data.Length - 16];
        Buffer.BlockCopy(data, 16, encryptedData, 0, encryptedData.Length);
        
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }

    private static byte[] DeriveKey(byte[] key, int length)
    {
        var hash = SHA256.HashData(key);
        var result = new byte[length];
        Buffer.BlockCopy(hash, 0, result, 0, Math.Min(hash.Length, length));
        return result;
    }
}