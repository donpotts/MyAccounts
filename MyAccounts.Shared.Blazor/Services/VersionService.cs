using System.Reflection;

namespace MyAccounts.Shared.Blazor.Services;

public class VersionService
{
    public string Version { get; }
    public string FullVersion { get; }

    public VersionService()
    {
        // Try to get the entry assembly first (works in Blazor WASM)
        // Fall back to looking for the MyAccounts.Blazor assembly
        var assembly = Assembly.GetEntryAssembly();
        
        if (assembly == null)
        {
            // Try to find the Blazor assembly by name
            assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "MyAccounts.Blazor")
                ?? Assembly.GetExecutingAssembly();
        }
        
        var version = assembly.GetName().Version;

        if (version != null)
        {
            // Display as Major.Minor.Build (e.g., 1.0.4)
            Version = $"{version.Major}.{version.Minor}.{version.Build}";
            FullVersion = version.ToString();
        }
        else
        {
            Version = "1.0.0";
            FullVersion = "1.0.0.0";
        }
    }
}
