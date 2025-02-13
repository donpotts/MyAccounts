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
public class BudgetExpenseController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<BudgetExpense>> Get()
    {
        var budgetExpenses = ctx.BudgetExpense;
        if (budgetExpenses == null || !budgetExpenses.Any())
        {
            return NotFound("No budget expenses found.");
        }
        return Ok(ctx.BudgetExpense);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetExpense>> GetAsync(long key)
    {
        var budgetExpense = await ctx.BudgetExpense.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetExpense == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(budgetExpense);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetExpense>> PostAsync(BudgetExpense budgetExpense)
    {
        var record = await ctx.BudgetExpense.FindAsync(budgetExpense.Id);
        if (record != null)
        {
            return Conflict();
        }

        await ctx.BudgetExpense.AddAsync(budgetExpense);

        await ctx.SaveChangesAsync();

        return Created($"/budgetaccount/{budgetExpense.Id}", budgetExpense);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetExpense>> PutAsync(long key, BudgetExpense update)
    {
        var budgetExpense = await ctx.BudgetExpense.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetExpense == null)
        {
            return NotFound();
        }

        ctx.Entry(budgetExpense).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(budgetExpense);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetExpense>> PatchAsync(long key, Delta<BudgetExpense> delta)
    {
        var budgetExpense = await ctx.BudgetExpense.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetExpense == null)
        {
            return NotFound();
        }

        delta.Patch(budgetExpense);

        await ctx.SaveChangesAsync();

        return Ok(budgetExpense);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var budgetExpense = await ctx.BudgetExpense.FindAsync(key);

        if (budgetExpense != null)
        {
            ctx.BudgetExpense.Remove(budgetExpense);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
