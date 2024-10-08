using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Services;
using MyAccounts.Shared.Models;
using System.Xml.Linq;

namespace MyAccounts.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext ctx;
    private readonly TransactionService transactionService;

    public TransactionController(ApplicationDbContext _ctx, TransactionService _transactionService)
    {
        ctx = _ctx;
        transactionService = _transactionService;
    }


    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IQueryable<Transaction>>> GetAsync(ODataQueryOptions<Transaction> options)
    {
        var balanceQueryable = ctx.Transaction.Include(x => x.Account).Include(x => x.Category).AsQueryable();
        List<Transaction> transactions = null;

        if (options.Filter?.RawValue.StartsWith("(Account/Name eq") ?? false)
        {
            balanceQueryable = (IQueryable<Transaction>)options.Filter.ApplyTo(balanceQueryable, new ODataQuerySettings());
        }
        transactions = await balanceQueryable.ToListAsync();
        transactionService.CalculateBalances(transactions);

        return Ok(transactions);
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

    [HttpGet("totals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> GetTotalsAsync([FromQuery] string accountName)
    {
        decimal? totalAmount = 0.00M;
        DateTime today = DateTime.Today;

        if (accountName == "all")
        {
            totalAmount = await ctx.Transaction
                                  .Where(t => t.Date <= DateOnly.FromDateTime(today))
                                  .SumAsync(t => t.Amount);
        }
        else
        {
            totalAmount = await ctx.Transaction
                                  .Where(t => t.Account.Name == accountName && t.Date <= DateOnly.FromDateTime(today))
                                  .SumAsync(t => t.Amount);
        }

        return Ok(totalAmount);
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

        await ctx.Transaction.AddAsync(transaction);

        await ctx.SaveChangesAsync();

        return Created($"/transaction/{transaction.Id}", transaction);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> PutAsync(long key, Transaction update)
    {
        var transaction = await ctx.Transaction.FirstOrDefaultAsync(x => x.Id == key);

        if (transaction == null)
        {
            return NotFound();
        }

        ctx.Entry(transaction).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(transaction);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> PatchAsync(long key, Delta<Transaction> delta)
    {
        var transaction = await ctx.Transaction.FirstOrDefaultAsync(x => x.Id == key);

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
