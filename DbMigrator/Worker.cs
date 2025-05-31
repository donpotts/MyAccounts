using ClosedXML.Parser;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using MyAccounts.Data;
using MyAccounts.Shared.Blazor.Pages;
using MyAccounts.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbMigrator
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var oldDb = scope.ServiceProvider.GetRequiredService<OldDbContext>();
            var newDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _logger.LogInformation("Starting migration...");

            // === Categories ===
            var categoryIdMap = new Dictionary<int, int>();
            var oldCategories = await oldDb.Category.ToListAsync(stoppingToken);

            foreach (var oldCategory in oldCategories)
            {
                var existingCategory = await newDb.Category
                    .FirstOrDefaultAsync(c => c.Name == oldCategory.Name, stoppingToken);

                if (existingCategory != null)
                {
                    categoryIdMap[(int)oldCategory.Id.Value] = (int)existingCategory.Id;
                    continue;
                }

                var newCategory = new Category
                {
                    Name = oldCategory.Name,
                };

                newDb.Category.Add(newCategory);
                await newDb.SaveChangesAsync(stoppingToken);

                categoryIdMap[(int)oldCategory.Id.Value] = (int)newCategory.Id;
            }

            // === Account Types ===
            var oldAccountTypes = await oldDb.AccountType.ToListAsync(stoppingToken);

            foreach (var oldType in oldAccountTypes)
            {
                var exists = await newDb.AccountType
                    .AnyAsync(a => a.Name == oldType.Name, stoppingToken);

                if (!exists)
                {
                    var newType = new AccountType
                    {
                        Name = oldType.Name,
                        Account = oldType.Account
                    };

                    newDb.AccountType.Add(newType);
                    await newDb.SaveChangesAsync(stoppingToken);
                }
            }

            // === Accounts ===
            var accountIdMap = new Dictionary<int, int>();
            var oldAccounts = await oldDb.Account.ToListAsync(stoppingToken);

            foreach (var oldAccount in oldAccounts)
            {
                var existingAccount = await newDb.Account
                    .FirstOrDefaultAsync(a => a.Name == oldAccount.Name, stoppingToken);

                if (existingAccount != null)
                {
                    accountIdMap[(int)oldAccount.Id.Value] = (int)existingAccount.Id;
                    continue;
                }

                var newAccount = new Account
                {
                    Name = oldAccount.Name,
                    AccountType = oldAccount.AccountTypeId.HasValue
                        ? await newDb.AccountType.FindAsync(oldAccount.AccountTypeId.Value, stoppingToken)
                        : null,
                    AccountTypeId = oldAccount.AccountTypeId,
                    Balance = oldAccount.Balance,
                    BudgetAccount = oldAccount.BudgetAccount,
                    Category = oldAccount.Category,
                    Date = oldAccount.Date,
                    Note = oldAccount.Note
                };

                newDb.Account.Add(newAccount);
                await newDb.SaveChangesAsync(stoppingToken);

                accountIdMap[(int)oldAccount.Id.Value] = (int)newAccount.Id;
            }

            // === Transactions ===
            var transactionIdMap = new Dictionary<int, int>();
            var oldTransactions = await oldDb.Transaction.ToListAsync(stoppingToken);

            foreach (var oldTxn in oldTransactions)
            {
                var existingTransaction = await newDb.Transaction
                    .FirstOrDefaultAsync(a => (a.Amount == oldTxn.Amount && a.Account == oldTxn.Account && a.Date == oldTxn.Date), stoppingToken);

                if (existingTransaction != null)
                {
                    accountIdMap[(int)oldTxn.Id.Value] = (int)existingTransaction.Id;
                    continue;
                }

                var newTransaction = new Transaction
                {
                    Payee = oldTxn.Payee,
                    Amount = oldTxn.Amount,
                    Balance = oldTxn.Balance,
                    Date = oldTxn.Date,
                    Description = oldTxn.Description,
                    AccountId = accountIdMap[(int)oldTxn.AccountId.Value],
                    CategoryId = oldTxn.CategoryId
                };

                newDb.Transaction.Add(newTransaction);
                await newDb.SaveChangesAsync(stoppingToken);
            }

            _logger.LogInformation("Migration complete.");
            _applicationLifetime.StopApplication();
        }
    }
}
