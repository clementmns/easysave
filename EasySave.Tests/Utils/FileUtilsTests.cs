using System.Runtime.InteropServices;
using System.Text;
using EasySave.Utils;

namespace EasySave.Tests.Utils;

public class FileUtilsTests
{
    [Fact]
    public void CopyFile_CreatesDestinationAndCopiesContent()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var destRoot = Path.Combine(root, "dest");
        var nestedDir = Path.Combine(sourceRoot, "nested");
        Directory.CreateDirectory(nestedDir);

        var sourceFile = Path.Combine(nestedDir, "file.txt");
        const string content = "hello";
        File.WriteAllText(sourceFile, content, Encoding.UTF8);

        try
        {
            var result = FileUtils.CopyFile(sourceFile, destRoot, sourceRoot);

            var expectedDest = Path.Combine(destRoot, "nested", "file.txt");
            Assert.True(result);
            Assert.True(File.Exists(expectedDest));
            Assert.Equal(content, File.ReadAllText(expectedDest, Encoding.UTF8));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void GetAllFiles_ReturnsFilesFromSubdirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var subDir = Path.Combine(root, "sub");
        Directory.CreateDirectory(subDir);

        var rootFile = Path.Combine(root, "root.txt");
        var subFile = Path.Combine(subDir, "sub.txt");
        File.WriteAllText(rootFile, "root");
        File.WriteAllText(subFile, "sub");

        try
        {
            var files = FileUtils.GetAllFiles(root);

            Assert.Equal(2, files.Count);
            Assert.Contains(files, file => file.FullName == rootFile);
            Assert.Contains(files, file => file.FullName == subFile);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void ConvertToUnc_ReturnsExpectedResultBasedOnPlatform()
    {
        var input = Path.GetTempPath();

        var result = FileUtils.ConvertToUnc(input);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.StartsWith("\\\\", result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public void GetLastModifiedDate_ReturnsValueForExistingFile()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "file.txt");
        File.WriteAllText(filePath, "data");

        try
        {
            var lastModified = FileUtils.GetLastModifiedDate(filePath);

            Assert.NotNull(lastModified);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void CreateDirectory_CreatesDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var created = FileUtils.CreateDirectory(root);

            Assert.True(created);
            Assert.True(Directory.Exists(root));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void GetFileSize_ReturnsByteLength()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "file.bin");
        var data = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(filePath, data);

        try
        {
            var size = FileUtils.GetFileSize(filePath);

            Assert.Equal(data.Length, size);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Fact]
    public void IsDirectory_ReturnsExpectedValues()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "file.txt");
        File.WriteAllText(filePath, "data");

        try
        {
            var isDir = FileUtils.IsDirectory(root);
            var isFileDir = FileUtils.IsDirectory(filePath);
            var isMissing = FileUtils.IsDirectory(Path.Combine(root, "missing"));

            Assert.True(isDir);
            Assert.False(isFileDir);
            Assert.Null(isMissing);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
