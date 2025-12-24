using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Services;
using MyAccounts.Shared.Models;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
[EnableRateLimiting("Fixed")]
public class BackupController(BackupService backupService, ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBackup()
    {
        var (data, fileName, success, error) = await backupService.CreateBackupAsync("Manual");

        if (!success)
        {
            return StatusCode(500, $"Backup failed: {error}");
        }

        return File(data, "application/sql", fileName);
    }

    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BackupLog>>> GetLogs([FromQuery] int limit = 50)
    {
        var logs = await dbContext.BackupLog
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RunScheduledBackup()
    {
        var filePath = await backupService.CreateScheduledBackupAsync();

        if (filePath != null)
        {
            return Ok(new { message = "Backup created successfully", path = filePath });
        }

        return StatusCode(500, "Backup failed");
    }
}
