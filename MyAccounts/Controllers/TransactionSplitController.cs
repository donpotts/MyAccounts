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
public class TransactionSplitController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<TransactionSplit>> Get()
    {
        return Ok(ctx.TransactionSplit.Include(x => x.Transaction).Include(x => x.Category));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionSplit>> GetAsync(long key)
    {
        var transactionSplit = await ctx.TransactionSplit.Include(x => x.Transaction).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == key);

        if (transactionSplit == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(transactionSplit);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TransactionSplit>> PostAsync(TransactionSplit transactionSplit)
    {
        var record = await ctx.TransactionSplit.FindAsync(transactionSplit.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        await ctx.TransactionSplit.AddAsync(transactionSplit);

        await ctx.SaveChangesAsync();

        return Created($"/transactionsplit/{transactionSplit.Id}", transactionSplit);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionSplit>> PutAsync(long key, TransactionSplit update)
    {
        var transactionSplit = await ctx.TransactionSplit.FirstOrDefaultAsync(x => x.Id == key);

        if (transactionSplit == null)
        {
            return NotFound();
        }

        ctx.Entry(transactionSplit).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(transactionSplit);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionSplit>> PatchAsync(long key, Delta<TransactionSplit> delta)
    {
        var transactionSplit = await ctx.TransactionSplit.FirstOrDefaultAsync(x => x.Id == key);

        if (transactionSplit == null)
        {
            return NotFound();
        }

        delta.Patch(transactionSplit);

        await ctx.SaveChangesAsync();

        return Ok(transactionSplit);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var transactionSplit = await ctx.TransactionSplit.FindAsync(key);

        if (transactionSplit != null)
        {
            ctx.TransactionSplit.Remove(transactionSplit);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
