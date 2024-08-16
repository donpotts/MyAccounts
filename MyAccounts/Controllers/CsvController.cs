using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyAccounts.Data;
using MyAccounts.Services;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class CsvController(CsvService csvService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostAsync(IFormFile file)
    {
        var extension = file.ContentType switch
        {
            "text/csv" => ".csv",
            _ => null
        };

        try
        {
            using var stream = file.OpenReadStream();
            return Ok($"\"{await csvService.SaveToUploadsAsync(extension, stream)}\"");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"\"{ex.Message}\"");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"\"{ex.Message}\"");
        }
    }

}