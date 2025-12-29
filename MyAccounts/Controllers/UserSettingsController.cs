using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Shared.Models;
using System.Security.Claims;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class UserSettingsController(ApplicationDbContext ctx) : ControllerBase
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User not found");

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSettings>>> GetAll()
    {
        var userId = GetUserId();
        var settings = await ctx.UserSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // If user has no settings, return defaults
        if (settings.Count == 0)
        {
            var defaultSettings = DefaultUserSettings.Defaults
                .Select(kvp => new UserSettings
                {
                    UserId = userId,
                    SettingKey = kvp.Key,
                    SettingValue = kvp.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();
            return Ok(defaultSettings);
        }

        return Ok(settings);
    }

    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSettings>> GetByKey(string key)
    {
        var userId = GetUserId();
        var setting = await ctx.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

        if (setting == null)
        {
            // Check if there's a default value for this key
            var defaultValue = DefaultUserSettings.GetDefault(key);
            if (defaultValue != null)
            {
                return Ok(new UserSettings
                {
                    UserId = userId,
                    SettingKey = key,
                    SettingValue = defaultValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            return NotFound();
        }

        return Ok(setting);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserSettings>> Upsert(string key, [FromBody] string value)
    {
        var userId = GetUserId();
        var setting = await ctx.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

        if (setting == null)
        {
            setting = new UserSettings
            {
                UserId = userId,
                SettingKey = key,
                SettingValue = value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await ctx.UserSettings.AddAsync(setting);
            await ctx.SaveChangesAsync();
            return Created($"/api/usersettings/{key}", setting);
        }

        setting.SettingValue = value;
        setting.UpdatedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        return Ok(setting);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string key)
    {
        var userId = GetUserId();
        var setting = await ctx.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SettingKey == key);

        if (setting != null)
        {
            ctx.UserSettings.Remove(setting);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }

    /// <summary>
    /// Initialize user settings with defaults if they don't have any settings yet.
    /// This can be called on first login or when needed.
    /// </summary>
    [HttpPost("initialize-defaults")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSettings>>> InitializeDefaults()
    {
        var userId = GetUserId();
        
        // Check if user already has any settings
        var existingSettings = await ctx.UserSettings
            .Where(s => s.UserId == userId)
            .Select(s => s.SettingKey)
            .ToListAsync();

        var newSettings = new List<UserSettings>();

        foreach (var kvp in DefaultUserSettings.Defaults)
        {
            // Only add settings that don't already exist
            if (!existingSettings.Contains(kvp.Key))
            {
                var setting = new UserSettings
                {
                    UserId = userId,
                    SettingKey = kvp.Key,
                    SettingValue = kvp.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await ctx.UserSettings.AddAsync(setting);
                newSettings.Add(setting);
            }
        }

        if (newSettings.Count > 0)
        {
            await ctx.SaveChangesAsync();
        }

        // Return all settings (existing + new)
        var allSettings = await ctx.UserSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();

        return Ok(allSettings);
    }

    /// <summary>
    /// Reset all user settings to defaults.
    /// Deletes existing settings and replaces with defaults.
    /// </summary>
    [HttpPost("reset-to-defaults")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSettings>>> ResetToDefaults()
    {
        var userId = GetUserId();
        
        // Delete all existing settings for this user
        var existingSettings = await ctx.UserSettings
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (existingSettings.Count > 0)
        {
            ctx.UserSettings.RemoveRange(existingSettings);
            await ctx.SaveChangesAsync();
        }

        // Create new settings from defaults
        var newSettings = new List<UserSettings>();

        foreach (var kvp in DefaultUserSettings.Defaults)
        {
            var setting = new UserSettings
            {
                UserId = userId,
                SettingKey = kvp.Key,
                SettingValue = kvp.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await ctx.UserSettings.AddAsync(setting);
            newSettings.Add(setting);
        }

        await ctx.SaveChangesAsync();

        return Ok(newSettings);
    }
}
