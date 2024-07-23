using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Shared.Models;
using System.Runtime.CompilerServices;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class TransactionController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Transaction>> Get()
    {
        return Ok(ctx.Transaction.Include(x => x.Account).Include(x => x.Category));
    }

    [HttpGet("totals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetTotalsAsync([FromQuery] string accountName)
    {
        decimal? totalAmount = 0.00M;
        if (accountName == "all")
        {
            totalAmount = await ctx.Transaction.SumAsync(t => t.Amount);
        }
        else
        {
            totalAmount = await ctx.Transaction.Where(t => t.Account.Name == accountName).SumAsync(t => t.Amount);
        }
        return Ok(totalAmount);
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> GetAsync(long key)
    {
        var transaction = await ctx.Transaction.Include(x => x.Account).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == key);

        if (transaction == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(transaction);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Transaction>> PostAsync(Transaction transaction)
    {
        var record = await ctx.Transaction.FindAsync(transaction.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        var category = transaction.Category;
        transaction.Category = null;

        await ctx.Transaction.AddAsync(transaction);

        if (category != null)
        {
            var newValues = await ctx.Category.Where(x => category.Select(y => y.Id).Contains(x.Id)).ToListAsync();
            transaction.Category = [..newValues];
        }

        await ctx.SaveChangesAsync();

        return Created($"/transaction/{transaction.Id}", transaction);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> PutAsync(long key, Transaction update)
    {
        var transaction = await ctx.Transaction.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == key);

        if (transaction == null)
        {
            return NotFound();
        }

        ctx.Entry(transaction).CurrentValues.SetValues(update);

        if (update.Category != null)
        {
            var updateValues = update.Category.Select(x => x.Id);
            transaction.Category ??= [];
            transaction.Category.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !transaction.Category.Select(y => y.Id).Contains(x));
            var newValues = await ctx.Category.Where(x => addValues.Contains(x.Id)).ToListAsync();
            transaction.Category.AddRange(newValues);
        }

        await ctx.SaveChangesAsync();

        return Ok(transaction);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> PatchAsync(long key, Delta<Transaction> delta)
    {
        var transaction = await ctx.Transaction.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == key);

        if (transaction == null)
        {
            return NotFound();
        }

        delta.Patch(transaction);

        await ctx.SaveChangesAsync();

        return Ok(transaction);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var transaction = await ctx.Transaction.FindAsync(key);

        if (transaction != null)
        {
            ctx.Transaction.Remove(transaction);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
