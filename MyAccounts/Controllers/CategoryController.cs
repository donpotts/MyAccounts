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
public class CategoryController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Category>> Get()
    {
        return Ok(ctx.Category
            .Include(x => x.Account)
            .Include(x => x.ParentCategory));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetAsync(long key)
    {
        var category = await ctx.Category
            .Include(x => x.Account)
            .Include(x => x.ParentCategory)
            .Include(x => x.SubCategories)
            .FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(category);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Category>> PostAsync(Category category)
    {
        var record = await ctx.Category.FindAsync(category.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        var account = category.Account;
        category.Account = null;

        await ctx.Category.AddAsync(category);

        if (account != null)
        {
            var newValues = await ctx.Account.Where(x => account.Select(y => y.Id).Contains(x.Id)).ToListAsync();
            category.Account = [..newValues];
        }

        await ctx.SaveChangesAsync();

        return Created($"/category/{category.Id}", category);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> PutAsync(long key, Category update)
    {
        var category = await ctx.Category
            .Include(x => x.Account)
            .Include(x => x.ParentCategory)
            .FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }

        ctx.Entry(category).CurrentValues.SetValues(update);

        if (update.Account != null)
        {
            var updateValues = update.Account.Select(x => x.Id);
            category.Account ??= [];
            category.Account.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !category.Account.Select(y => y.Id).Contains(x));
            var newValues = await ctx.Account.Where(x => addValues.Contains(x.Id)).ToListAsync();
            category.Account.AddRange(newValues);
        }

        await ctx.SaveChangesAsync();

        return Ok(category);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> PatchAsync(long key, Delta<Category> delta)
    {
        var category = await ctx.Category
            .Include(x => x.Account)
            .Include(x => x.ParentCategory)
            .FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }

        delta.Patch(category);

        await ctx.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var category = await ctx.Category
            .Include(x => x.SubCategories)
            .FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }

        if (category.SubCategories?.Any() == true)
        {
            return BadRequest("Cannot delete a category that has subcategories. Delete the subcategories first.");
        }

        var hasTransactions = await ctx.Transaction.AnyAsync(t => t.CategoryId == key);
        if (hasTransactions)
        {
            return BadRequest("Cannot delete a category that has transactions. Reassign or delete the transactions first.");
        }

        var hasTransactionSplits = await ctx.TransactionSplit.AnyAsync(ts => ts.CategoryId == key);
        if (hasTransactionSplits)
        {
            return BadRequest("Cannot delete a category that has transaction splits. Reassign or delete the transaction splits first.");
        }

        ctx.Category.Remove(category);
        await ctx.SaveChangesAsync();

        return NoContent();
    }
}
