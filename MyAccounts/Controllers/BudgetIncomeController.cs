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
public class BudgetIncomeController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<BudgetIncome>> Get()
    {
        var budgetIncomes = ctx.BudgetIncome;
        if (budgetIncomes == null || !budgetIncomes.Any())
        {
            return NotFound("No budget incomes found.");
        }
        return Ok(ctx.BudgetIncome);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetIncome>> GetAsync(int key)
    {
        var budgetIncome = await ctx.BudgetIncome.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetIncome == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(budgetIncome);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetIncome>> PostAsync(BudgetIncome budgetIncome)
    {
        var record = await ctx.BudgetIncome.FindAsync(budgetIncome.Id);
        if (record != null)
        {
            return Conflict();
        }

        await ctx.BudgetIncome.AddAsync(budgetIncome);

        await ctx.SaveChangesAsync();

        return Created($"/BudgetIncome/{budgetIncome.Id}", budgetIncome);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetIncome>> PutAsync(int key, BudgetIncome update)
    {
        var budgetIncome = await ctx.BudgetIncome.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetIncome == null)
        {
            return NotFound();
        }

        ctx.Entry(budgetIncome).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(budgetIncome);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetIncome>> PatchAsync(int key, Delta<BudgetIncome> delta)
    {
        var budgetIncome = await ctx.BudgetIncome.FirstOrDefaultAsync(x => x.Id == key);

        if (budgetIncome == null)
        {
            return NotFound();
        }

        delta.Patch(budgetIncome);

        await ctx.SaveChangesAsync();

        return Ok(budgetIncome);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int key)
    {
        var budgetIncome = await ctx.BudgetIncome.FindAsync(key);

        if (budgetIncome != null)
        {
            ctx.BudgetIncome.Remove(budgetIncome);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
