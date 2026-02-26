using System.Text;
using EasySave.Core.Utils;

namespace EasySave.Tests.Utils;

public class FileUtilsAdditionalTests
{
    // ── CopyFile: path traversal protection ──────────────────────────────────

    [Fact]
    public void CopyFile_ReturnsFalse_WhenRelativePathContainsTraversal()
    {
        // Create a file in some dir, then construct a sourceRoot that forces a ".." in the relative path.
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var inner = Path.Combine(root, "inner");
        Directory.CreateDirectory(inner);
        var sourceFile = Path.Combine(inner, "file.txt");
        File.WriteAllText(sourceFile, "data");

        var dst = Path.Combine(root, "dest");
        Directory.CreateDirectory(dst);

        try
        {
            // Passing a sourceRoot that is *inside* inner means the relative path will contain ".."
            // e.g. Path.GetRelativePath(inner + "/sub", inner/file.txt) = "../file.txt"
            var fakeRoot = Path.Combine(inner, "sub"); // sub does not exist → relative path will have ".."
            var result = FileUtils.CopyFile(sourceFile, dst, fakeRoot, null);

            Assert.False(result.Item1);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── GetFileSizeAndCount: single file ─────────────────────────────────────

    [Fact]
    public void GetFileSizeAndCount_SingleFile_ReturnsOneAndCorrectSize()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "single.bin");
        var data = new byte[128];
        File.WriteAllBytes(filePath, data);

        try
        {
            var (size, count) = FileUtils.GetFileSizeAndCount(filePath);

            Assert.Equal(128, size);
            Assert.Equal(1, count);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── GetFileSizeAndCount: directory ────────────────────────────────────────

    [Fact]
    public void GetFileSizeAndCount_Directory_ReturnsTotalSizeAndCount()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllBytes(Path.Combine(root, "a.bin"), new byte[10]);
        File.WriteAllBytes(Path.Combine(sub, "b.bin"), new byte[20]);

        try
        {
            var (size, count) = FileUtils.GetFileSizeAndCount(root);

            Assert.Equal(30, size);
            Assert.Equal(2, count);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── GetFileSizeAndCount: non-existent path ────────────────────────────────

    [Fact]
    public void GetFileSizeAndCount_NonExistentPath_ReturnsZeros()
    {
        var (size, count) = FileUtils.GetFileSizeAndCount("/nonexistent/path/xyz");

        Assert.Equal(0, size);
        Assert.Equal(0, count);
    }

    // ── GetFileSize: non-existent file ────────────────────────────────────────

    [Fact]
    public void GetFileSize_NonExistentFile_ReturnsZero()
    {
        var size = FileUtils.GetFileSize("/nonexistent/does_not_exist.bin");

        Assert.Equal(0, size);
    }

    // ── SeparatePriorityFiles ─────────────────────────────────────────────────

    [Fact]
    public void SeparatePriorityFiles_SeparatesCorrectly()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var txtFile = new FileInfo(Path.Combine(root, "a.txt"));
        var pdfFile = new FileInfo(Path.Combine(root, "b.pdf"));
        var docFile = new FileInfo(Path.Combine(root, "c.doc"));

        File.WriteAllText(txtFile.FullName, "a");
        File.WriteAllText(pdfFile.FullName, "b");
        File.WriteAllText(docFile.FullName, "c");

        var files = new List<FileInfo> { txtFile, pdfFile, docFile };
        var priorities = new List<string> { ".txt", ".pdf" };

        try
        {
            var (priority, nonPriority) = FileUtils.SeparatePriorityFiles(files, priorities);

            Assert.Equal(2, priority.Count);
            Assert.Single(nonPriority);
            Assert.Contains(priority, f => f.Extension == ".txt");
            Assert.Contains(priority, f => f.Extension == ".pdf");
            Assert.Contains(nonPriority, f => f.Extension == ".doc");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SeparatePriorityFiles_EmptyPriorityList_AllFilesAreNonPriority()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = new FileInfo(Path.Combine(root, "x.txt"));
        File.WriteAllText(file.FullName, "x");

        try
        {
            var (priority, nonPriority) = FileUtils.SeparatePriorityFiles([file], []);

            Assert.Empty(priority);
            Assert.Single(nonPriority);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    // ── HasPriorityFiles ──────────────────────────────────────────────────────

    [Fact]
    public void HasPriorityFiles_EmptyExtensions_ReturnsFalse()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "x.txt"), "x");

        try
        {
            var result = FileUtils.HasPriorityFiles(root, []);

            Assert.False(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void HasPriorityFiles_DirectoryWithMatchingFile_ReturnsTrue()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "important.docx"), "data");

        try
        {
            var result = FileUtils.HasPriorityFiles(root, [".docx"]);

            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void HasPriorityFiles_DirectoryWithNoMatchingFile_ReturnsFalse()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "readme.txt"), "data");

        try
        {
            var result = FileUtils.HasPriorityFiles(root, [".pdf"]);

            Assert.False(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void HasPriorityFiles_SingleFileWithMatchingExtension_ReturnsTrue()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "data.pdf");
        File.WriteAllText(file, "pdf content");

        try
        {
            var result = FileUtils.HasPriorityFiles(file, [".pdf"]);

            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void HasPriorityFiles_NonExistentPath_ReturnsFalse()
    {
        var result = FileUtils.HasPriorityFiles("/nonexistent/path", [".txt"]);

        Assert.False(result);
    }

    // ── GetAllFiles: empty directory ──────────────────────────────────────────

    [Fact]
    public void GetAllFiles_EmptyDirectory_ReturnsEmptyList()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var files = FileUtils.GetAllFiles(root);

            Assert.Empty(files);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetAllFiles_NonExistentDirectory_ReturnsEmptyList()
    {
        var files = FileUtils.GetAllFiles("/nonexistent/dir/xyz");

        Assert.Empty(files);
    }

    // ── GetLastModifiedDate: non-existent file ────────────────────────────────
    // FileInfo.LastWriteTime on a missing file does not throw; on Windows it
    // returns the file system epoch (1601-01-01). GetLastModifiedDate therefore
    // returns a non-null DateTime rather than null.

    [Fact]
    public void GetLastModifiedDate_NonExistentFile_ReturnsNonNullDateTime()
    {
        var result = FileUtils.GetLastModifiedDate("/nonexistent/file.txt");

        // The method wraps FileInfo which returns a placeholder date for missing
        // files rather than throwing, so a non-null value is returned.
        Assert.NotNull(result);
    }
}
