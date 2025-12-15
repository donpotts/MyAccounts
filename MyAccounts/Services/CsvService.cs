using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
using MyAccounts.Data;
using MyAccounts.Models;
using MyAccounts.Shared.Models;
using static MudBlazor.CategoryTypes;

namespace MyAccounts.Services;

public class CsvService(IWebHostEnvironment environment, ApplicationDbContext ctx)
{
    public async Task<(string filePath, int insertedCount)> SaveToUploadsAsync(string? extension, Stream csvFile, long accountId)
    {
        decimal? SumTransactionSplits = 00.0M;
        string todaySplit = "Split"+System.DateOnly.FromDateTime(System.DateTime.Now).ToString("yyyyMMdd");

        if (string.IsNullOrEmpty(extension) || extension != ".csv")
        {
            throw new ArgumentException("The file must be in CSV format.", nameof(extension));
        }

        string fileName;
        string filePath;

        // Use temp directory instead of wwwroot for Azure App Service compatibility
        // Azure App Service has read-only wwwroot, but temp directory is writable
        var uploadPath = Path.Combine(Path.GetTempPath(), "MyAccounts", "upload", "csv");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        do
        {
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            fileName = $"{randomFileName}{extension}";
            filePath = Path.Combine(uploadPath, fileName);
        }
        while (File.Exists(filePath));

        using (FileStream fs = new(filePath, FileMode.CreateNew))
        {
            await csvFile.CopyToAsync(fs);
        }

        string csvFilePath = filePath;

        // Detect CSV type by reading the first line
        string firstLine;
        using (var reader = new StreamReader(csvFilePath))
        {
            firstLine = await reader.ReadLineAsync() ?? string.Empty;
        }
        bool isGenericCreditCard = firstLine.Contains("Transaction Date") && firstLine.Contains("Card No.") && firstLine.Contains("Debit") && firstLine.Contains("Credit");

        var transactions = await QuickenTransactionImporter.ReadQuickenTransactionsAsync(csvFilePath);
        var bankTransactions = await BankTransactionImporter.ReadBankTransactionsAsync(csvFilePath);
        var genericCreditCardTransactions = isGenericCreditCard ? await BankTransactionImporter.ReadGenericCreditCardTransactionsAsync(csvFilePath) : null;
        var bankCreditCardTransactions = !isGenericCreditCard ? await BankTransactionImporter.ReadBankCreditCardTransactionsAsync(csvFilePath) : null;

        int insertedCount = 0;

        if (bankCreditCardTransactions != null)
        {
            foreach (var creditCardTransaction in bankCreditCardTransactions)
            {
                Console.WriteLine($"{creditCardTransaction.DateStart} - {creditCardTransaction.DateEnd} - {creditCardTransaction.Amount} - {creditCardTransaction.Description} - {creditCardTransaction.Type} - {creditCardTransaction.Category}");
                long? catId;
                
                // Use category from CSV if provided, otherwise use default
                if (!string.IsNullOrWhiteSpace(creditCardTransaction.Category))
                {
                    var category = await ctx.Category.FirstOrDefaultAsync(x => x.Name == creditCardTransaction.Category);
                    if (category == null)
                    {
                        category = new Category
                        {
                            Name = creditCardTransaction.Category
                        };
                        ctx.Category.Add(category);
                        await ctx.SaveChangesAsync();
                    }
                    catId = category.Id;
                }
                else if (creditCardTransaction.Amount > 0)
                {
                    catId = 1;
                }
                else
                {
                    catId = 35;
                }

                var existingTransaction = await ctx.Transaction
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => (t.Amount ?? 0) == creditCardTransaction.Amount * -1 && t.Date >= creditCardTransaction.DateStart);


                var newTransaction = new Transaction
                {
                    Date = creditCardTransaction.DateStart,
                    Payee = creditCardTransaction.Description,
                    Amount = creditCardTransaction.Amount * -1,
                    Description = "Bank Credit Card Transaction Import!",
                    AccountId = accountId,
                    CategoryId = catId,
                    Cleared = true
                };
                if (existingTransaction != null)
                {
                    // Update existing transaction
                    existingTransaction.Date = newTransaction.Date;
                    existingTransaction.Payee = newTransaction.Payee;
                    existingTransaction.Amount = newTransaction.Amount;
                    existingTransaction.Description = "Bank Credit Card Transaction Import Update!";
                    
                    // Update category if existing is uncategorized and CSV has a category
                    if (!string.IsNullOrWhiteSpace(creditCardTransaction.Category) && 
                        (existingTransaction.Category == null || 
                         existingTransaction.Category.Name == null ||
                         existingTransaction.Category.Name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase)))
                    {
                        existingTransaction.CategoryId = newTransaction.CategoryId;
                    }
                    
                    existingTransaction.AccountId = newTransaction.AccountId;

                    ctx.Transaction.Update(existingTransaction);
                    Console.WriteLine($"Updated Transaction: {existingTransaction.Date} - {existingTransaction.Payee} - {existingTransaction.Amount} - {existingTransaction.CategoryId} - {existingTransaction.AccountId}");
                }
                else
                {
                    // Insert new transaction
                    ctx.Transaction.Add(newTransaction);
                    insertedCount++;
                    Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
                }

                await ctx.SaveChangesAsync();
            }
        }
        
        if (genericCreditCardTransactions != null)
        {
            foreach (var creditCardTransaction in genericCreditCardTransactions)
            {
                Console.WriteLine($"{creditCardTransaction.TransactionDate} - {creditCardTransaction.PostedDate} - {creditCardTransaction.Debit} - {creditCardTransaction.Credit} - {creditCardTransaction.Description} - {creditCardTransaction.Category}");
                long? catId;
                decimal amount = creditCardTransaction.Debit ?? 0;
                if (amount > 0)
                {
                    amount = -amount; // Debits are negative (expenses)
                }
                else
                {
                    amount = creditCardTransaction.Credit ?? 0; // Credits are positive (payments/refunds)
                }

                // Use category from CSV if provided, otherwise use default
                if (!string.IsNullOrWhiteSpace(creditCardTransaction.Category))
                {
                    var category = await ctx.Category.FirstOrDefaultAsync(x => x.Name == creditCardTransaction.Category);
                    if (category == null)
                    {
                        category = new Category
                        {
                            Name = creditCardTransaction.Category
                        };
                        ctx.Category.Add(category);
                        await ctx.SaveChangesAsync();
                    }
                    catId = category.Id;
                }
                else if (creditCardTransaction.Debit > 0)
                {
                    catId = 1;
                }
                else
                {
                    catId = 35;
                }

                var existingTransaction = await ctx.Transaction
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => (t.Amount ?? 0) == amount && t.Date == creditCardTransaction.TransactionDate);

                var newTransaction = new Transaction
                {
                    Date = creditCardTransaction.TransactionDate,
                    Payee = creditCardTransaction.Description,
                    Amount = amount,
                    Description = "Credit Card Transaction Import!",
                    AccountId = accountId,
                    CategoryId = catId,
                    Cleared = true
                };
                if (existingTransaction != null)
                {
                    // Update existing transaction
                    existingTransaction.Date = newTransaction.Date;
                    existingTransaction.Payee = newTransaction.Payee;
                    existingTransaction.Amount = newTransaction.Amount;
                    existingTransaction.Description = "Credit Card Transaction Import Update!";
                    
                    // Update category if existing is uncategorized and CSV has a category
                    if (!string.IsNullOrWhiteSpace(creditCardTransaction.Category) && 
                        (existingTransaction.Category == null || 
                         existingTransaction.Category.Name == null ||
                         existingTransaction.Category.Name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase)))
                    {
                        existingTransaction.CategoryId = newTransaction.CategoryId;
                    }
                    
                    existingTransaction.AccountId = newTransaction.AccountId;

                    ctx.Transaction.Update(existingTransaction);
                    Console.WriteLine($"Updated Transaction: {existingTransaction.Date} - {existingTransaction.Payee} - {existingTransaction.Amount} - {existingTransaction.CategoryId} - {existingTransaction.AccountId}");
                }
                else
                {
                    // Insert new transaction
                    ctx.Transaction.Add(newTransaction);
                    insertedCount++;
                    Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
                }

                await ctx.SaveChangesAsync();
            }
        }

        if (bankTransactions != null)
        {
            foreach (var bankTransaction in bankTransactions)
            {
                Console.WriteLine($"{bankTransaction.Date} - {bankTransaction.Time} - {bankTransaction.Amount} - {bankTransaction.Description} - {bankTransaction.Type} - {bankTransaction.Category}");
                long? catId;
                
                // Use category from CSV if provided, otherwise use default
                if (!string.IsNullOrWhiteSpace(bankTransaction.Category))
                {
                    var category = await ctx.Category.FirstOrDefaultAsync(x => x.Name == bankTransaction.Category);
                    if (category == null)
                    {
                        category = new Category
                        {
                            Name = bankTransaction.Category
                        };
                        ctx.Category.Add(category);
                        await ctx.SaveChangesAsync();
                    }
                    catId = category.Id;
                }
                else if (bankTransaction.Amount > 0)
                {
                    catId = 6;
                }
                else
                {
                    catId = 5;
                }

                var existingTransaction = await ctx.Transaction
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => (t.Amount == bankTransaction.Amount && t.Date == bankTransaction.Date));
                var newTransaction = new Transaction
                {
                    Date = bankTransaction.Date,
                    Payee = bankTransaction.Description,
                    Amount = bankTransaction.Amount,
                    AccountId = accountId == 0 ? 4 : accountId, // fallback to 4 if not provided
                    CategoryId = catId,
                    Cleared = true
                };
                if (existingTransaction != null)
                {
                    // Update existing transaction
                    existingTransaction.Date = newTransaction.Date;
                    existingTransaction.Payee = newTransaction.Payee;
                    existingTransaction.Amount = newTransaction.Amount;
                    
                    // Update category if existing is uncategorized and CSV has a category
                    if (!string.IsNullOrWhiteSpace(bankTransaction.Category) && 
                        (existingTransaction.Category == null || 
                         existingTransaction.Category.Name == null ||
                         existingTransaction.Category.Name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase)))
                    {
                        existingTransaction.CategoryId = newTransaction.CategoryId;
                    }
                    
                    existingTransaction.AccountId = newTransaction.AccountId;

                    ctx.Transaction.Update(existingTransaction);
                    Console.WriteLine($"Updated Transaction: {existingTransaction.Date} - {existingTransaction.Payee} - {existingTransaction.Amount} - {existingTransaction.CategoryId} - {existingTransaction.AccountId}");
                }
                else
                {
                    // Insert new transaction
                    ctx.Transaction.Add(newTransaction);
                    insertedCount++;
                    Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
                }

                await ctx.SaveChangesAsync();
            }
        }

        long? transactionId = null;
        long? splitAccountId = null;
        string? payeeSplit = null;
        DateOnly? transactionDate = null;

        Console.WriteLine($"Total transactions={transactions.Count}");
        Console.WriteLine($"Total Split transactions={transactions.Where(x => x.Split == "S").Count()}");
        Console.WriteLine($"Total Non-Split transactions={transactions.Where(x => x.Split != "S").Count()}");

        foreach (var transaction in transactions)
        {
            Console.WriteLine($"{transaction.Date} - {transaction.Payee} - {transaction.Amount} - {transaction.Category} - {transaction.Account}");

            var category = await ctx.Category.FirstOrDefaultAsync(x => x.Name == transaction.Category);
            int categoryId;
            if (category == null)
            {
                // Create a new Category if it doesn't exist
                category = new Category
                {
                    Name = transaction.Category
                };
                ctx.Category.Add(category);
                await ctx.SaveChangesAsync();
                categoryId = (int)category.Id;
            }
            else
            {
                categoryId = (int)category.Id;
            }

            // Always use provided accountId
            var newTransaction = new Transaction
            {
                Date = transaction.Date,
                Payee = transaction.Payee,
                Amount = transaction.Amount,
                CategoryId = categoryId,
                AccountId = accountId == 0 ? accountId : accountId,
                Cleared = true
            };

            if (transaction.Split == "S")
            {
                newTransaction.Description = todaySplit;
            }

            var existingTransaction = await ctx.Transaction
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => (t.Amount == newTransaction.Amount && t.Date == newTransaction.Date));

            if (existingTransaction != null)
            {
                // Update existing transaction
                existingTransaction.Date = newTransaction.Date;
                existingTransaction.Payee = newTransaction.Payee;
                existingTransaction.Amount = newTransaction.Amount;
                
                // Update category if existing is uncategorized and CSV has a category
                if (!string.IsNullOrWhiteSpace(transaction.Category) && 
                    (existingTransaction.Category == null || 
                     existingTransaction.Category.Name == null ||
                     existingTransaction.Category.Name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase)))
                {
                    existingTransaction.CategoryId = newTransaction.CategoryId;
                }
                
                existingTransaction.AccountId = newTransaction.AccountId;

                ctx.Transaction.Update(existingTransaction);
                Console.WriteLine($"Updated Transaction: {existingTransaction.Date} - {existingTransaction.Payee} - {existingTransaction.Amount} - {existingTransaction.CategoryId} - {existingTransaction.AccountId}");
            }
            else
            {
                // Insert new transaction
                ctx.Transaction.Add(newTransaction);
                insertedCount++;
                Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
            }

            await ctx.SaveChangesAsync();
        }

        var splitTrans = ctx.Transaction.Where(x => x.Description == todaySplit)
                                .GroupBy(x => new { x.Date, x.Payee, x.Account.Id });

        foreach (var group in splitTrans)
        {
            Console.WriteLine($"Group: Date={group.Key.Date}, Payee={group.Key.Payee}, Account={group.Key.Id}");
                        
            var groupSum = group.Sum(x => x.Amount);
            Console.WriteLine($"Group Sum: {groupSum}");

            var newTrans = new Transaction
            {
                Date = group.Key.Date,
                Payee = group.Key.Payee,
                AccountId = group.Key.Id,
                CategoryId = 4,
                Amount = groupSum,
            };

            await ctx.Transaction.AddAsync(newTrans);
            await ctx.SaveChangesAsync();

            var newTransId = newTrans.Id;

            foreach (var trans in group)
            {
                Console.WriteLine($"Transaction: Id={trans.Id}, Amount={trans.Amount}, Description={trans.Description}");
                await ctx.TransactionSplit.AddAsync(new TransactionSplit
                {
                    Amount = trans.Amount,
                    CategoryId = trans.CategoryId,
                    TransactionId = newTransId,
                });
            }
        }
        await ctx.SaveChangesAsync();

        var deleteSplitTransactions = ctx.Transaction.Where(x => x.Description == todaySplit);

        ctx.Transaction.RemoveRange(deleteSplitTransactions);

        await ctx.SaveChangesAsync();

        // Clean up the temporary file after processing
        try
        {
            if (File.Exists(csvFilePath))
            {
                File.Delete(csvFilePath);
            }
        }
        catch
        {
            // Ignore cleanup errors - temp files will be cleaned up by OS eventually
        }

        return (fileName, insertedCount);
    }
}
