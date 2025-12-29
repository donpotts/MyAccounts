using System.Reflection;

namespace MyAccounts.Shared.Blazor.Services;

public class VersionService
{
    public string Version { get; }
    public string FullVersion { get; }
    public string BuildDate { get; }

    public VersionService()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        if (version != null)
        {
            // Display as Major.Minor.Build (e.g., 1.0.25364)
            Version = $"{version.Major}.{version.Minor}.{version.Build}";
            FullVersion = version.ToString();
            
            // Try to parse build date from version (Build = YYDDD, Revision = HHMM)
            BuildDate = TryParseBuildDate(version.Build, version.Revision);
        }
        else
        {
            Version = "1.0.0";
            FullVersion = "1.0.0.0";
            BuildDate = "Unknown";
        }
    }

    private static string TryParseBuildDate(int build, int revision)
    {
        try
        {
            // Build format: YYDDD (e.g., 25364 = 2025, day 364)
            // Revision format: HHMM (e.g., 1430 = 14:30)
            if (build >= 25001 && build <= 99366)
            {
                int year = 2000 + (build / 1000);
                int dayOfYear = build % 1000;
                
                if (dayOfYear >= 1 && dayOfYear <= 366)
                {
                    var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
                    
                    if (revision >= 0 && revision <= 2359)
                    {
                        int hour = revision / 100;
                        int minute = revision % 100;
                        if (hour < 24 && minute < 60)
                        {
                            var time = new TimeSpan(hour, minute, 0);
                            return date.Add(time).ToString("MMM dd, yyyy HH:mm") + " UTC";
                        }
                    }
                    
                    return date.ToString("MMM dd, yyyy");
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        
        return "Unknown";
    }
}
