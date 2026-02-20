using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft;

public static class Program
{
    private const int TIMESTAMP_TOLERANCE_SECONDS = 2;
    private const string KEY = "953cce052755512752a654d330a506ad4296aff67219bb7f706fb50f878268f0";
    
    public static void Main(string[] args)
    {
        var timestamp = Environment.GetEnvironmentVariable("EASYSAVE_TIMESTAMP");
        var signatureBase64 = Environment.GetEnvironmentVariable("EASYSAVE_SIGNATURE");
        var publicKeyPath = Environment.GetEnvironmentVariable("EASYSAVE_PUBLIC_KEY_PATH");
        
        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signatureBase64) || string.IsNullOrEmpty(publicKeyPath))
        {
            Environment.Exit(-1);
        }

        if (!IsTimestampFresh(timestamp))
        {
            Environment.Exit(-1);
        }

        if (!IsSignatureValid(timestamp, signatureBase64, publicKeyPath))
        {
            Environment.Exit(-1);
        }

        try
        {
            if (args.Length < 4)
            {
                Environment.Exit(-1);
            }

            var algorithmInput = args[0].ToLower();
            var actionInput= args[1].ToLower();
            var sourcePath = args[2];
            var destinationPath = args[3];

            var algorithm = algorithmInput switch
            {
                "xor" => CryptoAlgorithm.Xor,
                "aes" => CryptoAlgorithm.Aes,
                _ => throw new ArgumentException("Invalid algorithm. Use 'xor' or 'aes'.")
            };
            
            var isEncryption = actionInput switch
            {
                "encrypt" => true,
                "decrypt" => false,
                _ => throw new ArgumentException("Invalid action. Use 'encrypt' or 'decrypt'.")
            };

            var fileManager = new FileManager(sourcePath, destinationPath, KEY, algorithm, isEncryption);
            fileManager.TransformFile();
        }
        catch (Exception)
        {
            Environment.Exit(-99);
        }
    }
    
    private static bool IsTimestampFresh(string timestamp)
    {
        var currentTime  = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var receivedTime = long.Parse(timestamp);
        return Math.Abs(currentTime - receivedTime) <= TIMESTAMP_TOLERANCE_SECONDS;
    }
    
    private static bool IsSignatureValid(string timestamp, string signatureBase64, string publicKeyPath)
    {
        if (!File.Exists(publicKeyPath))
            return false;

        var publicKeyXml = File.ReadAllText(publicKeyPath);

        using var rsa = RSA.Create();
        rsa.FromXmlString(publicKeyXml);

        var data      = Encoding.UTF8.GetBytes(timestamp);
        var signature = Convert.FromBase64String(signatureBase64);
        
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    }
}
