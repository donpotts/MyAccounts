using System.Text.Json;
using MyAccounts.Shared.Models;

namespace MyAccounts.Shared.Blazor.Services;

public class UserSettingsService(AppService appService)
{
    private readonly Dictionary<string, string?> _cache = [];
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            var settings = await appService.ListUserSettingsAsync();
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    if (setting.SettingKey != null)
                    {
                        _cache[setting.SettingKey] = setting.SettingValue;
                    }
                }
            }
            
            // If no settings were loaded, populate cache with defaults
            if (_cache.Count == 0)
            {
                foreach (var kvp in DefaultUserSettings.Defaults)
                {
                    _cache[kvp.Key] = kvp.Value;
                }
            }
            
            _initialized = true;
        }
        catch
        {
            // On error, use defaults
            foreach (var kvp in DefaultUserSettings.Defaults)
            {
                _cache[kvp.Key] = kvp.Value;
            }
            _initialized = true;
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var value = await appService.GetUserSettingValueAsync(key);
        
        // If no value from server, check defaults
        if (value == null)
        {
            value = DefaultUserSettings.GetDefault(key);
        }
        
        _cache[key] = value;
        return value;
    }

    public async Task SetAsync(string key, string value)
    {
        await appService.SetUserSettingAsync(key, value);
        _cache[key] = value;
    }

    public async Task DeleteAsync(string key)
    {
        await appService.DeleteUserSettingAsync(key);
        _cache.Remove(key);
    }

    // Column visibility helpers
    public async Task<HashSet<string>> GetHiddenColumnsAsync(string gridName)
    {
        var value = await GetAsync($"grid.{gridName}.hiddenColumns");
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<HashSet<string>>(value) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SetHiddenColumnsAsync(string gridName, HashSet<string> hiddenColumns)
    {
        var value = JsonSerializer.Serialize(hiddenColumns);
        await SetAsync($"grid.{gridName}.hiddenColumns", value);
    }

    public async Task<bool> IsColumnHiddenAsync(string gridName, string columnName)
    {
        var hidden = await GetHiddenColumnsAsync(gridName);
        return hidden.Contains(columnName);
    }

    public async Task SetColumnVisibilityAsync(string gridName, string columnName, bool visible)
    {
        var hidden = await GetHiddenColumnsAsync(gridName);
        if (visible)
        {
            hidden.Remove(columnName);
        }
        else
        {
            hidden.Add(columnName);
        }
        await SetHiddenColumnsAsync(gridName, hidden);
    }

    // Page size helpers
    public async Task<int> GetPageSizeAsync(string gridName, int defaultSize = 25)
    {
        var value = await GetAsync($"grid.{gridName}.pageSize");
        if (int.TryParse(value, out var size))
        {
            return size;
        }
        return defaultSize;
    }

    public async Task SetPageSizeAsync(string gridName, int pageSize)
    {
        await SetAsync($"grid.{gridName}.pageSize", pageSize.ToString());
    }

    // Generic JSON helpers for complex settings
    public async Task<T?> GetJsonAsync<T>(string key)
    {
        var value = await GetAsync(key);
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetJsonAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await SetAsync(key, json);
    }

    // Column name customization helpers
    public async Task<Dictionary<string, string>> GetColumnNamesAsync(string gridName)
    {
        var value = await GetAsync($"grid.{gridName}.columnNames");
        if (string.IsNullOrEmpty(value))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public async Task SetColumnNamesAsync(string gridName, Dictionary<string, string> columnNames)
    {
        var value = JsonSerializer.Serialize(columnNames);
        await SetAsync($"grid.{gridName}.columnNames", value);
    }

    public async Task<string> GetColumnNameAsync(string gridName, string columnKey, string defaultName)
    {
        var names = await GetColumnNamesAsync(gridName);
        return names.TryGetValue(columnKey, out var customName) ? customName : defaultName;
    }

    public async Task SetColumnNameAsync(string gridName, string columnKey, string customName)
    {
        var names = await GetColumnNamesAsync(gridName);
        names[columnKey] = customName;
        await SetColumnNamesAsync(gridName, names);
    }

    // Column order helpers
    public async Task<List<string>> GetColumnOrderAsync(string gridName)
    {
        var value = await GetAsync($"grid.{gridName}.columnOrder");
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SetColumnOrderAsync(string gridName, List<string> columnOrder)
    {
        var value = JsonSerializer.Serialize(columnOrder);
        await SetAsync($"grid.{gridName}.columnOrder", value);
    }

    // Get complete column configuration
    public async Task<ColumnConfig> GetColumnConfigAsync(string gridName)
    {
        var hiddenTask = GetHiddenColumnsAsync(gridName);
        var namesTask = GetColumnNamesAsync(gridName);
        var orderTask = GetColumnOrderAsync(gridName);

        await Task.WhenAll(hiddenTask, namesTask, orderTask);

        return new ColumnConfig
        {
            HiddenColumns = await hiddenTask,
            ColumnNames = await namesTask,
            ColumnOrder = await orderTask
        };
    }

    // Save complete column configuration
    public async Task SetColumnConfigAsync(string gridName, ColumnConfig config)
    {
        await SetHiddenColumnsAsync(gridName, config.HiddenColumns);
        await SetColumnNamesAsync(gridName, config.ColumnNames);
        await SetColumnOrderAsync(gridName, config.ColumnOrder);
    }

    public async Task ResetToDefaultsAsync()
    {
        await appService.ResetUserSettingsToDefaultsAsync();
        ClearCache();
        await InitializeAsync();
    }

    public void ClearCache()
    {
        _cache.Clear();
        _initialized = false;
    }
}

public class ColumnConfig
{
    public HashSet<string> HiddenColumns { get; set; } = [];
    public Dictionary<string, string> ColumnNames { get; set; } = new();
    public List<string> ColumnOrder { get; set; } = [];
}
