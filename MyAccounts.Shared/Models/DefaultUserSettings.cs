using System.Collections.Generic;

namespace MyAccounts.Shared.Models;

/// <summary>
/// Provides default user settings for users who haven't configured their own.
/// These settings will be applied automatically when a user has no settings.
/// </summary>
public static class DefaultUserSettings
{
    /// <summary>
    /// Default settings that will be applied to all new users or users without settings.
    /// </summary>
    public static readonly Dictionary<string, string> Defaults = new()
    {
        // Transaction Grid - Hidden Columns
        ["grid.TransactionGrid.hiddenColumns"] = "[\"Description\",\"Account\"]",
        
        // Transaction Grid - Custom Column Names
        ["grid.TransactionGrid.columnNames"] = "{\"Balance\":\"Account Balance\"}",
        
        // Transaction Grid - Column Order
        ["grid.TransactionGrid.columnOrder"] = "[\"Date\",\"Payee\",\"Cleared\",\"Category\",\"Description\",\"Account\",\"DepositAmount\",\"PaymentAmount\",\"Balance\"]"
    };

    /// <summary>
    /// Gets the default value for a setting key, or null if not defined.
    /// </summary>
    public static string? GetDefault(string settingKey)
    {
        return Defaults.TryGetValue(settingKey, out var value) ? value : null;
    }

    /// <summary>
    /// Gets all default setting keys.
    /// </summary>
    public static IEnumerable<string> GetAllKeys()
    {
        return Defaults.Keys;
    }
}
