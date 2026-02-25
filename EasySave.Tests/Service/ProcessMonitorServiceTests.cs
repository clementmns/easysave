using EasySave.Core.Model;
using EasySave.Core.Service;
using EasySave.Tests.Model;

namespace EasySave.Tests.Service;

public class ProcessMonitorServiceTests
{
    public ProcessMonitorServiceTests()
    {
        try { _ = SettingsService.GetInstance; }
        catch { SettingsService.Init(new TestsProperties()); }
    }

    // ── Non-existent process name ─────────────────────────────────────────────

    [Fact]
    public void IsBusinessSoftwareRunning_ReturnsFalse_WhenProcessNotRunning()
    {
        // Use a highly-unlikely process name to ensure it is never running.
        SettingsService.GetInstance.SetBusinessSoftwareProcessName("zzz_nonexistent_process_xyz_123");

        var result = ProcessMonitorService.IsBusinessSoftwareRunning;

        Assert.False(result);
    }

    // ── Empty / whitespace process name ──────────────────────────────────────

    [Fact]
    public void IsBusinessSoftwareRunning_ReturnsFalse_WhenProcessNameIsEmpty()
    {
        SettingsService.GetInstance.SetBusinessSoftwareProcessName("");

        var result = ProcessMonitorService.IsBusinessSoftwareRunning;

        Assert.False(result);
    }

    [Fact]
    public void IsBusinessSoftwareRunning_ReturnsFalse_WhenProcessNameIsWhitespace()
    {
        SettingsService.GetInstance.SetBusinessSoftwareProcessName("   ");

        var result = ProcessMonitorService.IsBusinessSoftwareRunning;

        Assert.False(result);
    }

    // ── Comma-separated list with non-existent processes ─────────────────────

    [Fact]
    public void IsBusinessSoftwareRunning_ReturnsFalse_WhenAllNamesInListAreNonExistent()
    {
        SettingsService.GetInstance.SetBusinessSoftwareProcessName("zzz_fake_a,zzz_fake_b;zzz_fake_c");

        var result = ProcessMonitorService.IsBusinessSoftwareRunning;

        Assert.False(result);
    }
}
