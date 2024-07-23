using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Models;
using MyAccounts.Shared.Models;

namespace MyAccounts.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AccountType> AccountType => Set<AccountType>();
    public DbSet<Account> Account => Set<Account>();
    public DbSet<Category> Category => Set<Category>();
    public DbSet<Transaction> Transaction => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountType>()
            .HasMany(x => x.Account);
        modelBuilder.Entity<Account>()
            .Property(e => e.Balance)
            .HasConversion<double>();
        modelBuilder.Entity<Account>()
            .Property(e => e.Balance)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Account>()
            .HasOne(x => x.AccountType);
        modelBuilder.Entity<Category>()
            .HasMany(x => x.Transaction);
        modelBuilder.Entity<Transaction>()
            .Property(e => e.Amount)
            .HasConversion<double>();
        modelBuilder.Entity<Transaction>()
            .Property(e => e.Amount)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Transaction>()
            .Property(e => e.Balance)
            .HasConversion<double>();
        modelBuilder.Entity<Transaction>()
            .Property(e => e.Balance)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Transaction>()
            .HasOne(x => x.Account);
        modelBuilder.Entity<Transaction>()
            .HasMany(x => x.Category);
    }
}
