using MyAccounts.Shared.Blazor.Services;
using System.Text.Json;

namespace MyAccounts.Maui.Blazor.Services;

public class MauiStorageService : IStorageService
{
    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await SecureStorage.Default.GetAsync(key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json);
    }

    public Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);

        return SecureStorage.Default.SetAsync(key, json);
    }

    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.RemoveAll();

        return Task.CompletedTask;
    }
}
