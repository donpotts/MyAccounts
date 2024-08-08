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

            // Add the new Transaction to the DbSet
            ctx.Transaction.Add(newTransaction);

            // Create a new TransactionSplit
            var newTransactionSplit = new TransactionSplit
            {
                Amount = transaction.Amount,
                CategoryId = categoryId,
            };

            // Save the changes to the database
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Inserted Transaction: {newTransaction.Date} - {newTransaction.Payee} - {newTransaction.Amount} - {newTransaction.CategoryId} - {newTransaction.AccountId}");
        }
        return $"/upload/csv/{fileName}";
    }
}
