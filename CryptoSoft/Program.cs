using System;

namespace CryptoSoft;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Vérifier les arguments de ligne de commande
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <algorithm> <action> <sourcePath> <destinationPath> <key>");
                Console.WriteLine("  algorithm: xor ou aes");
                Console.WriteLine("  action: encrypt ou decrypt");
                Environment.Exit(-1);
                return;
            }

            // Parser les arguments
            string algorithmStr = args[0].ToLower();
            string actionStr = args[1].ToLower();
            string sourcePath = args[2];
            string destinationPath = args[3];
            string key = args[4];

            // Convertir l'algorithme
            CryptoAlgorithm algorithm = algorithmStr switch
            {
                "xor" => CryptoAlgorithm.Xor,
                "aes" => CryptoAlgorithm.Aes,
                _ => throw new ArgumentException("Algorithme invalide. Utilisez 'xor' ou 'aes'.")
            };

            // Convertir l'action
            bool isEncryption = actionStr switch
            {
                "encrypt" => true,
                "decrypt" => false,
                _ => throw new ArgumentException("Action invalide. Utilisez 'encrypt' ou 'decrypt'.")
            };

            // Exécuter la transformation
            var fileManager = new FileManager(sourcePath, destinationPath, key, algorithm, isEncryption);
            int elapsedTime = fileManager.TransformFile();

            if (elapsedTime >= 0)
            {
                Console.WriteLine($"SUCCESS:{elapsedTime}");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("ERROR:Opération échouée");
                Environment.Exit(-2);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR:{e.Message}");
            Environment.Exit(-99);
        }
    }
}