using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Shared.Models;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class BudgetMonthController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<BudgetMonth>> Get()
    {
        var budgetMonths = ctx.BudgetMonth;
        if (budgetMonths == null || !budgetMonths.Any())
        {
            return NotFound("No budget months found.");
        }
        return Ok(ctx.BudgetMonth);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetMonth>> GetAsync(int key)
    {
        var budgetMonth = await ctx.BudgetMonth.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetMonth == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(budgetMonth);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetMonth>> PostAsync(BudgetMonth budgetMonth)
    {
        var record = await ctx.BudgetMonth.FindAsync(budgetMonth.Id);
        if (record != null)
        {
            return Conflict();
        }

        await ctx.BudgetMonth.AddAsync(budgetMonth);

        await ctx.SaveChangesAsync();

        return Created($"/budgetMonth/{budgetMonth.Id}", budgetMonth);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetMonth>> PutAsync(long key, BudgetMonth update)
    {
        var budgetMonth = await ctx.BudgetMonth.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetMonth == null)
        {
            return NotFound();
        }

        ctx.Entry(budgetMonth).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(budgetMonth);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetMonth>> PatchAsync(int key, Delta<BudgetMonth> delta)
    {
        var budgetMonth = await ctx.BudgetMonth.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetMonth == null)
        {
            return NotFound();
        }

        delta.Patch(budgetMonth);

        await ctx.SaveChangesAsync();

        return Ok(budgetMonth);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int key)
    {
        var budgetMonth = await ctx.BudgetMonth.FindAsync(key);

        if (budgetMonth != null)
        {
            ctx.BudgetMonth.Remove(budgetMonth);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
