using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Models;
using MyAccounts.Shared.Models;

namespace MyAccounts.Services;

public class CsvService(IWebHostEnvironment environment, ApplicationDbContext ctx)
{
    private readonly string webRootPath = environment.WebRootPath;

    public async Task<string> SaveToUploadsAsync(string? extension, Stream csvFile)
    {
        decimal? SumTransactionSplits = 00.0M;
        string todaySplit = "Split"+System.DateOnly.FromDateTime(System.DateTime.Now).ToString("yyyyMMdd");

        if (string.IsNullOrEmpty(extension) || extension != ".csv")
        {
            throw new ArgumentException("The file must be in CSV format.", nameof(extension));
        }

        string fileName;
        string filePath;

        var uploadPath = Path.Combine(webRootPath, "upload", "csv");

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

        Uri csvUri = new($"/upload/csv/{fileName}", UriKind.Relative);

        string csvFilePath = filePath;

        var transactions = await QuickenTransactionImporter.ReadQuickenTransactionsAsync(csvFilePath);

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

            // Get the AccountId
            var account = await ctx.Account.FirstOrDefaultAsync(x => x.Name == transaction.Account);
            int accountId;
            if (account == null)
            {
                // Create a new Account if it doesn't exist
                account = new Account
                {
                    Name = transaction.Account
                };
                ctx.Account.Add(account);
                await ctx.SaveChangesAsync();
                accountId = (int)account.Id;
            }
            else
            {
                accountId = (int)account.Id;
            }

            // Create a new Transaction
            var newTransaction = new Transaction
            {
                Date = transaction.Date,
                Payee = transaction.Payee,
                Amount = transaction.Amount,
                CategoryId = categoryId,
                AccountId = accountId
            };

            if (transaction.Split == "S")
            {
                newTransaction.Description = todaySplit;
            }

            ctx.Transaction.Add(newTransaction);
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
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

        return $"/upload/csv/{fileName}";
    }
}
