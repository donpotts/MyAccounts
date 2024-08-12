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
                newTransaction.Description = "Split";
            }

            ctx.Transaction.Add(newTransaction);
            await ctx.SaveChangesAsync();

            //if (transaction.Split == "S")
            //{
            //    if (SumTransactionSplits == 0)
            //    {
            //        newTransaction.Amount = 0;
            //        ctx.Transaction.Add(newTransaction);
            //        await ctx.SaveChangesAsync();
            //        transactionId = newTransaction.Id;
            //        splitAccountId = newTransaction.Account.Id;
            //        payeeSplit = newTransaction.Payee.ToString();
            //        transactionDate = newTransaction.Date;
            //    }

            //    // Create a new TransactionSplit
            //    var newTransactionSplit = new TransactionSplit
            //    {
            //        Amount = transaction.Amount,
            //        CategoryId = categoryId,
            //        TransactionId = transactionId,
            //        Notes = "Transaction Split from Import.",
            //    };

            //    ctx.TransactionSplit.Add(newTransactionSplit);
            //    await ctx.SaveChangesAsync();

            //    SumTransactionSplits += transaction.Amount;
            //}
            //else
            //{
            //    if (SumTransactionSplits < 0)
            //    {
            //        var updateTransaction = await ctx.Transaction.FirstOrDefaultAsync(t => t.Id == transactionId);

            //        if (updateTransaction != null)
            //        {
            //            updateTransaction.Date = transactionDate;
            //            updateTransaction.Payee = payeeSplit;
            //            updateTransaction.AccountId = splitAccountId;
            //            updateTransaction.Amount = SumTransactionSplits;
            //            updateTransaction.CategoryId = 4;

            //            SumTransactionSplits = 0;
            //            splitAccountId = null;
            //            payeeSplit = null;
            //            transactionDate = null;

            //            ctx.Transaction.Update(updateTransaction);
            //            await ctx.SaveChangesAsync();
            //        }
            //    }
            //    else
            //    {
            //        ctx.Transaction.Add(newTransaction);
            //        await ctx.SaveChangesAsync();
            //    }
            //}
            

            Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
        }

        //Get all Split without Transaction and group by Date, Payee, Account, sum(Amount)

        //var mySplits = ctx.TransactionSplit.Where(st => st.TransactionId == null)
        //    .GroupBy(st => new { st.Transaction.Date, st.Transaction.Payee, st.Transaction.Account })
        //    .Select(g => new
        //    {
        //        Date = g.Key.Date,
        //        Payee = g.Key.Payee,
        //        Account = g.Key.Account,
        //        TotalAmount = g.Sum(st => st.Amount)
        //    })
        //    .ToList();

        //foreach (var split in mySplits)
        //{
        //    var newTransaction = new Transaction
        //    {
        //        Date = split.Date,
        //        Payee = split.Payee,
        //        Amount = split.TotalAmount,
        //        CategoryId = 4,
        //        AccountId = split.Account.Id,
        //    };
        //    ctx.Transaction.Add(newTransaction);
        //}
        //await ctx.SaveChangesAsync();

        var splitTransactions = ctx.Transaction.Where(st => st.Description == "Split")
                .GroupBy(st => new { st.Date, st.Payee, st.Account })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Payee = g.Key.Payee,
                    Account = g.Key.Account,
                    TotalAmount = g.Sum(st => st.Amount)
                })
                .ToList();

        foreach (var splitTransaction in splitTransactions)
        {
            var newTransaction = new Transaction
            {
                Date = splitTransaction.Date,
                Payee = splitTransaction.Payee,
                Amount = splitTransaction.TotalAmount,
                CategoryId = 4,
                AccountId = splitTransaction.Account.Id,
            };
            ctx.Transaction.Add(newTransaction);
            await ctx.SaveChangesAsync();
        }

        return $"/upload/csv/{fileName}";
    }
}
