using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class DesktopExportTests
{
    [Fact]
    public void TargetSelectionPreservesLinuxDefaultsAndSeparatesWindowsOutputs()
    {
        Assert.Equal("linux-x64", Options.Parse(["export-client"]).ExportTarget);
        Assert.Equal("windows-x64", Options.Parse(["ci-windows"]).ExportTarget);
        var windows = DesktopExport.For(Options.Parse(["export-client", "--target", "windows-x64"]).ExportTarget);
        Assert.Equal("windows-client", windows.Kind(false));
        Assert.Equal("windows-production-client", windows.Kind(true));
        Assert.Equal("Windows Production Client", windows.ClientPreset(true));
        Assert.Equal("odot.exe", windows.Executable);
        Assert.Equal("client", DesktopExport.For("linux-x64").Kind(false));
        Assert.Throws<ArgumentException>(() => Options.Parse(["export-client", "--target", "win-arm64"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["ci", "--target", "windows-x64"]));
        Assert.Equal("windows-x64", Options.Parse(["check-steam-extension", "--offline", "--exported", "--target", "windows-x64"]).ExportTarget);
    }
}
