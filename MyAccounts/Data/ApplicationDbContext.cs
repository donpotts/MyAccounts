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
    public DbSet<TransactionSplit> TransactionSplit => Set<TransactionSplit>();

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
        modelBuilder.Entity<Account>()
            .HasMany(x => x.Category);
        modelBuilder.Entity<Category>()
            .HasMany(x => x.Account);
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
            .HasOne(x => x.Category);
        modelBuilder.Entity<TransactionSplit>()
            .Property(e => e.Amount)
            .HasConversion<double>();
        modelBuilder.Entity<TransactionSplit>()
            .Property(e => e.Amount)
            .HasPrecision(19, 4);
        modelBuilder.Entity<TransactionSplit>()
            .HasOne(x => x.Transaction);
        modelBuilder.Entity<TransactionSplit>()
            .HasOne(x => x.Category);

        // Make Category Name unique
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Make Account Name unique
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Name)
            .IsUnique();
    }

    //public List<Category> GetSortedCategories()
    //{
    //    return Category.OrderBy(c => c.Name).ToList();
    //}

    //public List<Account> GetSortedAccounts()
    //{
    //    return Account.OrderBy(c => c.Name).ToList();
    //}
}
