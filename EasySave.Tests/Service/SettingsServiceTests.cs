using EasyLog;
using EasySave.Core.Model;
using EasySave.Core.Service;

namespace EasySave.Tests.Service;

/// <summary>
/// Tests for <see cref="SettingsService"/>.
/// Because SettingsService is a singleton that cannot be reset between test runs,
/// every test class uses an isolated temp directory and the try/catch guard pattern
/// already established in the test suite.  We only test state mutations on the
/// already-initialised singleton.
/// </summary>
public class SettingsServiceTests
{
    private static readonly string TempRoot =
        Path.Combine(Path.GetTempPath(), "EasySaveTests_Settings_" + Guid.NewGuid().ToString("N"));

    public SettingsServiceTests()
    {
        try
        {
            _ = SettingsService.GetInstance;
        }
        catch
        {
            SettingsService.Init(new IsolatedProperties());
        }
    }

    // ── Init idempotency ──────────────────────────────────────────────────────

    [Fact]
    public void Init_CalledTwice_ReturnsSameInstance()
    {
        SettingsService.Init(new IsolatedProperties());
        var first = SettingsService.GetInstance;

        SettingsService.Init(new IsolatedProperties());
        var second = SettingsService.GetInstance;

        Assert.Same(first, second);
    }

    // ── GetInstance before Init ───────────────────────────────────────────────

    // (The singleton is already initialised in the constructor, so we can only
    //  verify that GetInstance returns a non-null value after Init.)
    [Fact]
    public void GetInstance_ReturnsNonNull()
    {
        var instance = SettingsService.GetInstance;

        Assert.NotNull(instance);
    }

    // ── SetCryptedExtensions ──────────────────────────────────────────────────

    [Fact]
    public void SetCryptedExtensions_UpdatesSettings()
    {
        var extensions = new List<string> { ".txt", ".pdf" };

        SettingsService.GetInstance.SetCryptedExtensions(extensions);

        Assert.Equal(extensions, SettingsService.GetInstance.Settings.CryptExtensions);
    }

    [Fact]
    public void SetCryptedExtensions_EmptyList_ClearsExtensions()
    {
        SettingsService.GetInstance.SetCryptedExtensions([".doc"]);
        SettingsService.GetInstance.SetCryptedExtensions([]);

        Assert.Empty(SettingsService.GetInstance.Settings.CryptExtensions);
    }

    // ── SetPriorityExtensions ─────────────────────────────────────────────────

    [Fact]
    public void SetPriorityExtensions_UpdatesSettings()
    {
        var extensions = new List<string> { ".docx", ".xlsx" };

        SettingsService.GetInstance.SetPriorityExtensions(extensions);

        Assert.Equal(extensions, SettingsService.GetInstance.Settings.PriorityExtensions);
    }

    // ── SetBusinessSoftwareProcessName ────────────────────────────────────────

    [Fact]
    public void SetBusinessSoftwareProcessName_UpdatesSettings()
    {
        SettingsService.GetInstance.SetBusinessSoftwareProcessName("notepad");

        Assert.Equal("notepad", SettingsService.GetInstance.Settings.BusinessSoftwareProcessName);
    }

    // ── SetMaxTransferSizeForParallel ─────────────────────────────────────────

    [Fact]
    public void SetMaxTransferSizeForParallel_UpdatesSettings()
    {
        SettingsService.GetInstance.SetMaxTransferSizeForParallel(2048);

        Assert.Equal(2048, SettingsService.GetInstance.Settings.MaxTransferSizeForParallel);
    }

    // ── SetCryptoSoftPath ─────────────────────────────────────────────────────

    [Fact]
    public void SetCryptoSoftPath_UpdatesSettings()
    {
        SettingsService.GetInstance.SetCryptoSoftPath("/some/path/cryptosoft.exe");

        Assert.Equal("/some/path/cryptosoft.exe", SettingsService.GetInstance.Settings.CryptoSoftPath);
    }

    // ── ChangeLogFormat ───────────────────────────────────────────────────────

    [Fact]
    public void ChangeLogFormat_ToXml_UpdatesSettings()
    {
        SettingsService.GetInstance.ChangeLogFormat(LogFormat.Xml);

        Assert.Equal(LogFormat.Xml, SettingsService.GetInstance.Settings.LogFormat);
    }

    [Fact]
    public void ChangeLogFormat_ToJson_UpdatesSettings()
    {
        SettingsService.GetInstance.ChangeLogFormat(LogFormat.Json);

        Assert.Equal(LogFormat.Json, SettingsService.GetInstance.Settings.LogFormat);
    }

    [Fact]
    public void ChangeLogFormat_SameFormat_DoesNotThrow()
    {
        var current = SettingsService.GetInstance.Settings.LogFormat;

        // Should be a no-op and must not throw.
        SettingsService.GetInstance.ChangeLogFormat(current);

        Assert.Equal(current, SettingsService.GetInstance.Settings.LogFormat);
    }

    // ── SetLanguage ───────────────────────────────────────────────────────────

    [Fact]
    public void SetLanguage_ValidCode_UpdatesSettings()
    {
        SettingsService.GetInstance.SetLanguage("en-US");

        Assert.Equal("en-US", SettingsService.GetInstance.Settings.Language);
    }

    [Fact]
    public void SetLanguage_French_UpdatesSettings()
    {
        SettingsService.GetInstance.SetLanguage("fr-FR");

        Assert.Equal("fr-FR", SettingsService.GetInstance.Settings.Language);
    }

    [Fact]
    public void SetLanguage_InvalidCode_Throws()
    {
        Assert.Throws<Exception>(() =>
            SettingsService.GetInstance.SetLanguage("xx-INVALID"));
    }

    // ── Settings persisted to disk ────────────────────────────────────────────

    [Fact]
    public void Settings_AppSaveDirectory_IsNotNull()
    {
        // AppSaveDirectory comes from IAppProperties; TestsProperties returns "".
        // The setting should be a non-null string (even if empty is acceptable for tests).
        var dir = SettingsService.GetInstance.Settings.AppSaveDirectory;

        Assert.NotNull(dir);
    }

    private sealed class IsolatedProperties : IAppProperties
    {
        public string AppSaveDirectory { get; } =
            Path.Combine(Path.GetTempPath(), "EasySave_Settings_" + Guid.NewGuid().ToString("N"));
        public int MaxJobs { get; } = 5;
        public string? CryptoSoftPath { get; } = null;
    }
}
