using System.Reflection;

namespace MyAccounts.Shared.Blazor.Services;

public class VersionService
{
    public string Version { get; }
    public string FullVersion { get; }

    public VersionService()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        Version = version != null
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "1.0.0";

        FullVersion = version?.ToString() ?? "1.0.0.0";
    }
}
