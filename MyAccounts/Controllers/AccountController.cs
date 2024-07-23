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
public class AccountController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Account>> Get()
    {
        return Ok(ctx.Account.Include(x => x.AccountType));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> GetAsync(long key)
    {
        var account = await ctx.Account.Include(x => x.AccountType).FirstOrDefaultAsync(x => x.Id == key);

        if (account == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(account);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Account>> PostAsync(Account account)
    {
        var record = await ctx.Account.FindAsync(account.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        await ctx.Account.AddAsync(account);

        await ctx.SaveChangesAsync();

        return Created($"/account/{account.Id}", account);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> PutAsync(long key, Account update)
    {
        var account = await ctx.Account.FirstOrDefaultAsync(x => x.Id == key);

        if (account == null)
        {
            return NotFound();
        }

        ctx.Entry(account).CurrentValues.SetValues(update);

        await ctx.SaveChangesAsync();

        return Ok(account);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> PatchAsync(long key, Delta<Account> delta)
    {
        var account = await ctx.Account.FirstOrDefaultAsync(x => x.Id == key);

        if (account == null)
        {
            return NotFound();
        }

        delta.Patch(account);

        await ctx.SaveChangesAsync();

        return Ok(account);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var account = await ctx.Account.FindAsync(key);

        if (account != null)
        {
            ctx.Account.Remove(account);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }
}
