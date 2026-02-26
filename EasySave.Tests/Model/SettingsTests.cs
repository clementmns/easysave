using System.Text.Json;
using EasyLog;
using EasySave.Core.Model;

namespace EasySave.Tests.Model;

public class SettingsTests
{
    // ── Default values ────────────────────────────────────────────────────────

    [Fact]
    public void DefaultSettings_HasExpectedLanguage()
    {
        var settings = new Settings();

        Assert.Equal("en-US", settings.Language);
    }

    [Fact]
    public void DefaultSettings_LogFormatIsJson()
    {
        var settings = new Settings();

        Assert.Equal(LogFormat.Json, settings.LogFormat);
    }

    [Fact]
    public void DefaultSettings_LogModeIsLocal()
    {
        var settings = new Settings();

        Assert.Equal(LogMode.Local, settings.LogMode);
    }

    [Fact]
    public void DefaultSettings_CryptExtensionsIsEmpty()
    {
        var settings = new Settings();

        Assert.Empty(settings.CryptExtensions);
    }

    [Fact]
    public void DefaultSettings_PriorityExtensionsIsEmpty()
    {
        var settings = new Settings();

        Assert.Empty(settings.PriorityExtensions);
    }

    [Fact]
    public void DefaultSettings_LogServerHostIsLocalhost()
    {
        var settings = new Settings();

        Assert.Null(settings.LogServerHost);
    }

    [Fact]
    public void DefaultSettings_LogServerPortIs5092()
    {
        var settings = new Settings();

        Assert.Null(settings.LogServerPort);
    }

    [Fact]
    public void DefaultSettings_MaxTransferSizeIs5096()
    {
        var settings = new Settings();

        Assert.Equal(5096, settings.MaxTransferSizeForParallel);
    }

    [Fact]
    public void DefaultSettings_BusinessSoftwareIsCalculatorApp()
    {
        var settings = new Settings();

        Assert.Equal("CalculatorApp", settings.BusinessSoftwareProcessName);
    }

    // ── JSON round-trip ───────────────────────────────────────────────────────

    [Fact]
    public void JsonRoundTrip_PreservesAllPersistedProperties()
    {
        var original = new Settings
        {
            Language = "fr-FR",
            LogFormat = LogFormat.Xml,
            LogMode = LogMode.Local,
            LogServerHost = "192.168.1.1",
            LogServerPort = 9999,
            CryptExtensions = [".txt", ".pdf"],
            PriorityExtensions = [".docx"],
            BusinessSoftwareProcessName = "notepad",
            MaxTransferSizeForParallel = 1024,
            Version = "9.9.9"
        };

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Settings>(json, options)!;

        Assert.Equal(original.Language, deserialized.Language);
        Assert.Equal(original.LogFormat, deserialized.LogFormat);
        Assert.Equal(original.LogServerHost, deserialized.LogServerHost);
        Assert.Equal(original.LogServerPort, deserialized.LogServerPort);
        Assert.Equal(original.CryptExtensions, deserialized.CryptExtensions);
        Assert.Equal(original.PriorityExtensions, deserialized.PriorityExtensions);
        Assert.Equal(original.BusinessSoftwareProcessName, deserialized.BusinessSoftwareProcessName);
        Assert.Equal(original.MaxTransferSizeForParallel, deserialized.MaxTransferSizeForParallel);
        Assert.Equal(original.Version, deserialized.Version);
    }

    [Fact]
    public void JsonRoundTrip_MaxJobsIsIgnored()
    {
        // MaxJobs is [JsonIgnore] – it must not appear in the serialized JSON.
        var settings = new Settings { MaxJobs = 42 };

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Serialize(settings, options);

        Assert.DoesNotContain("MaxJobs", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void JsonRoundTrip_AppSaveDirectoryIsIgnored()
    {
        // AppSaveDirectory is [JsonIgnore] – must not appear in the serialized JSON.
        var settings = new Settings { AppSaveDirectory = "/some/path" };

        var json = JsonSerializer.Serialize(settings);

        Assert.DoesNotContain("AppSaveDirectory", json, StringComparison.OrdinalIgnoreCase);
    }
}
