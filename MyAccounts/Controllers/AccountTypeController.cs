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
public class AccountTypeController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<AccountType>> Get()
    {
        return Ok(ctx.AccountType.Include(x => x.Account));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountType>> GetAsync(long key)
    {
        var accountType = await ctx.AccountType.Include(x => x.Account).FirstOrDefaultAsync(x => x.Id == key);

        if (accountType == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(accountType);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountType>> PostAsync(AccountType accountType)
    {
        var record = await ctx.AccountType.FindAsync(accountType.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        var account = accountType.Account;
        accountType.Account = null;

        await ctx.AccountType.AddAsync(accountType);

        if (account != null)
        {
            var newValues = await ctx.Account.Where(x => account.Select(y => y.Id).Contains(x.Id)).ToListAsync();
            accountType.Account = [..newValues];
        }

        await ctx.SaveChangesAsync();

        return Created($"/accounttype/{accountType.Id}", accountType);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountType>> PutAsync(long key, AccountType update)
    {
        var accountType = await ctx.AccountType.Include(x => x.Account).FirstOrDefaultAsync(x => x.Id == key);

        if (accountType == null)
        {
            return NotFound();
        }

        ctx.Entry(accountType).CurrentValues.SetValues(update);

        if (update.Account != null)
        {
            var updateValues = update.Account.Select(x => x.Id);
            accountType.Account ??= [];
            accountType.Account.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !accountType.Account.Select(y => y.Id).Contains(x));
            var newValues = await ctx.Account.Where(x => addValues.Contains(x.Id)).ToListAsync();
            accountType.Account.AddRange(newValues);
        }

        await ctx.SaveChangesAsync();

        return Ok(accountType);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountType>> PatchAsync(long key, Delta<AccountType> delta)
    {
        var accountType = await ctx.AccountType.Include(x => x.Account).FirstOrDefaultAsync(x => x.Id == key);

        if (accountType == null)
        {
            return NotFound();
        }

        delta.Patch(accountType);

        await ctx.SaveChangesAsync();

        return Ok(accountType);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var accountType = await ctx.AccountType.FindAsync(key);

        if (accountType != null)
        {
            ctx.AccountType.Remove(accountType);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
