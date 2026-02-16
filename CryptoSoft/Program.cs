namespace CryptoSoft;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <algorithm> <action> <sourcePath> <destinationPath> <key>");
                Console.WriteLine("  algorithm: xor ou aes");
                Console.WriteLine("  action: encrypt ou decrypt");
                Console.WriteLine("  sourcePath: file source path");
                Console.WriteLine("  destinationPath: directory destination path");
                Console.WriteLine("  key: sha256 encryption key");
                Environment.Exit(-1);
                return;
            }

            var algorithmInput = args[0].ToLower();
            var actionInput = args[1].ToLower();
            var sourcePath = args[2];
            var destinationPath = args[3];
            var key = args[4];

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

            var fileManager = new FileManager(sourcePath, destinationPath, key, algorithm, isEncryption);
            fileManager.TransformFile();
        }
        catch (Exception)
        {
            Environment.Exit(-99);
        }
    }
}