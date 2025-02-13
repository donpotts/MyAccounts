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
public class BudgetAccountController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<BudgetAccount>> Get()
    {
        var budgetAccounts = ctx.BudgetAccount;
        if (budgetAccounts == null || !budgetAccounts.Any())
        {
            return NotFound("No budget accounts found.");
        }
        return Ok(ctx.BudgetAccount);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetAccount>> GetAsync(long key)
    {
        var budgetAccount = await ctx.BudgetAccount.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetAccount == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(budgetAccount);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetAccount>> PostAsync(BudgetAccount budgetAccount)
    {
        var record = await ctx.BudgetAccount.FindAsync(budgetAccount.Id);
        if (record != null)
        {
            return Conflict();
        }

        await ctx.BudgetAccount.AddAsync(budgetAccount);

        await ctx.SaveChangesAsync();

        return Created($"/budgetaccount/{budgetAccount.Id}", budgetAccount);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetAccount>> PutAsync(long key, BudgetAccount update)
    {
        var budgetAccount = await ctx.BudgetAccount.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetAccount == null)
        {
            return NotFound();
        }

        ctx.Entry(budgetAccount).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(budgetAccount);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetAccount>> PatchAsync(long key, Delta<BudgetAccount> delta)
    {
        var budgetAccount = await ctx.BudgetAccount.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetAccount == null)
        {
            return NotFound();
        }

        delta.Patch(budgetAccount);

        await ctx.SaveChangesAsync();

        return Ok(budgetAccount);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var budgetAccount = await ctx.BudgetAccount.FindAsync(key);

        if (budgetAccount != null)
        {
            ctx.BudgetAccount.Remove(budgetAccount);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
