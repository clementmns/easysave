using CryptoSoft;

namespace EasySave.Tests.CryptoSoft;

public class FileManagerTests
{
    // ── XOR idempotency (encrypt then encrypt = original) ────────────────────

    [Fact]
    public void XorMethod_IsIdempotent_EncryptThenEncryptEqualsOriginal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var srcFile = Path.Combine(root, "original.bin");
        var encFile = Path.Combine(root, "encrypted.bin");
        var decFile = Path.Combine(root, "decrypted.bin");

        var original = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        File.WriteAllBytes(srcFile, original);

        try
        {
            // Encrypt
            var encryptor = new FileManager(srcFile, encFile, "secretkey", CryptoAlgorithm.Xor, true);
            encryptor.TransformFile();
            Assert.True(File.Exists(encFile));

            // Decrypt (XOR is symmetric – encrypt again with the same key)
            var decryptor = new FileManager(encFile, decFile, "secretkey", CryptoAlgorithm.Xor, false);
            decryptor.TransformFile();
            Assert.True(File.Exists(decFile));

            var result = File.ReadAllBytes(decFile);
            Assert.Equal(original, result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Empty key – TransformFile is a no-op ─────────────────────────────────

    [Fact]
    public void TransformFile_EmptyKey_DoesNotCreateDestinationFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var srcFile = Path.Combine(root, "src.txt");
        var dstFile = Path.Combine(root, "dst.txt");
        File.WriteAllText(srcFile, "data");

        try
        {
            var fm = new FileManager(srcFile, dstFile, "", CryptoAlgorithm.Xor, true);
            fm.TransformFile();

            // Empty key → early return, no destination file.
            Assert.False(File.Exists(dstFile));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Missing source file – no exception, just prints error ────────────────

    [Fact]
    public void TransformFile_MissingSourceFile_DoesNotThrow()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var dstFile = Path.Combine(root, "dst.txt");

        try
        {
            var fm = new FileManager("/nonexistent/file.bin", dstFile, "key", CryptoAlgorithm.Xor, true);

            // Should not throw – just log the error.
            fm.TransformFile();

            Assert.False(File.Exists(dstFile));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── AES round-trip ────────────────────────────────────────────────────────

    [Fact]
    public void AesEncryptThenDecrypt_ProducesOriginalBytes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var srcFile = Path.Combine(root, "plain.bin");
        var encFile = Path.Combine(root, "cipher.bin");
        var decFile = Path.Combine(root, "restored.bin");

        var original = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        File.WriteAllBytes(srcFile, original);

        try
        {
            var encryptor = new FileManager(srcFile, encFile, "MyStrongKey!", CryptoAlgorithm.Aes, true);
            encryptor.TransformFile();
            Assert.True(File.Exists(encFile));

            var decryptor = new FileManager(encFile, decFile, "MyStrongKey!", CryptoAlgorithm.Aes, false);
            decryptor.TransformFile();
            Assert.True(File.Exists(decFile));

            var result = File.ReadAllBytes(decFile);
            Assert.Equal(original, result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── AES encryption actually changes bytes ─────────────────────────────────

    [Fact]
    public void AesEncrypt_ProducesDifferentBytesFromOriginal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var srcFile = Path.Combine(root, "plain.bin");
        var encFile = Path.Combine(root, "cipher.bin");
        var original = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(srcFile, original);

        try
        {
            new FileManager(srcFile, encFile, "key", CryptoAlgorithm.Aes, true).TransformFile();

            var encrypted = File.ReadAllBytes(encFile);
            Assert.NotEqual(original, encrypted);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── XOR: output differs from input ───────────────────────────────────────

    [Fact]
    public void XorEncrypt_ProducesDifferentBytesFromInput()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var srcFile = Path.Combine(root, "plain.bin");
        var encFile = Path.Combine(root, "cipher.bin");
        var original = new byte[] { 65, 66, 67, 68 }; // "ABCD"
        File.WriteAllBytes(srcFile, original);

        try
        {
            new FileManager(srcFile, encFile, "key123", CryptoAlgorithm.Xor, true).TransformFile();

            var encrypted = File.ReadAllBytes(encFile);
            Assert.NotEqual(original, encrypted);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── Destination directory is created if missing ───────────────────────────

    [Fact]
    public void TransformFile_CreatesDestinationDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var srcFile = Path.Combine(root, "src.txt");
        var dstFile = Path.Combine(root, "subdir", "output.txt");

        Directory.CreateDirectory(root);
        File.WriteAllText(srcFile, "hello");

        try
        {
            new FileManager(srcFile, dstFile, "k", CryptoAlgorithm.Xor, true).TransformFile();

            Assert.True(File.Exists(dstFile));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
