using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;
using MyAccounts.Shared.Models;

namespace MyAccounts.Services;

public class BackupService(IServiceScopeFactory scopeFactory, ILogger<BackupService> logger)
{
    private static readonly string BackupDirectory = Path.Combine(Path.GetTempPath(), "MyAccounts", "backups");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<(byte[] Data, string FileName, bool Success, string? Error)> CreateBackupAsync(string backupType)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fileName = $"myaccounts-backup-{timestamp}.json";
        byte[] data = [];
        bool success = false;
        string? error = null;

        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Export all data tables
            var backup = new BackupData
            {
                ExportedAt = DateTime.UtcNow,
                Version = "1.0",
                AccountTypes = await dbContext.AccountType.ToListAsync(),
                Categories = await dbContext.Category.ToListAsync(),
                Accounts = await dbContext.Account.ToListAsync(),
                Transactions = await dbContext.Transaction.ToListAsync(),
                TransactionSplits = await dbContext.TransactionSplit.ToListAsync(),
                BudgetAccounts = await dbContext.BudgetAccount.ToListAsync(),
                BudgetMonths = await dbContext.BudgetMonth.ToListAsync(),
                BudgetExpenses = await dbContext.BudgetExpense.ToListAsync(),
                BudgetIncomes = await dbContext.BudgetIncome.ToListAsync()
            };

            var json = JsonSerializer.Serialize(backup, JsonOptions);
            data = System.Text.Encoding.UTF8.GetBytes(json);
            success = true;

            logger.LogInformation("Backup created successfully: {RecordCount} total records",
                backup.AccountTypes.Count + backup.Categories.Count + backup.Accounts.Count +
                backup.Transactions.Count + backup.TransactionSplits.Count +
                backup.BudgetAccounts.Count + backup.BudgetMonths.Count +
                backup.BudgetExpenses.Count + backup.BudgetIncomes.Count);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            logger.LogError(ex, "Backup failed");
        }

        // Log the backup
        await LogBackupAsync(fileName, data.Length, backupType, success, error);

        return (data, fileName, success, error);
    }

    public async Task<string?> CreateScheduledBackupAsync()
    {
        // Create backup directory if it doesn't exist
        Directory.CreateDirectory(BackupDirectory);

        // Get day of week for file naming (0=Sunday, 6=Saturday)
        var dayOfWeek = (int)DateTime.Now.DayOfWeek;
        var fileName = $"backup-day{dayOfWeek}.json";
        var filePath = Path.Combine(BackupDirectory, fileName);

        var (data, _, success, error) = await CreateBackupAsync("Scheduled");

        if (success && data.Length > 0)
        {
            await File.WriteAllBytesAsync(filePath, data);
            logger.LogInformation("Scheduled backup saved to {FilePath} ({Size} bytes)", filePath, data.Length);
            return filePath;
        }

        return null;
    }

    private async Task LogBackupAsync(string fileName, long fileSizeBytes, string backupType, bool success, string? errorMessage)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var log = new BackupLog
            {
                CreatedAt = DateTime.UtcNow,
                FileName = fileName,
                FileSizeBytes = fileSizeBytes,
                BackupType = backupType,
                Success = success,
                ErrorMessage = errorMessage
            };

            dbContext.BackupLog.Add(log);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log backup");
        }
    }

    public static string GetBackupDirectory() => BackupDirectory;
}

public class BackupData
{
    public DateTime ExportedAt { get; set; }
    public string Version { get; set; } = "1.0";
    public List<AccountType> AccountTypes { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
    public List<Account> Accounts { get; set; } = [];
    public List<Transaction> Transactions { get; set; } = [];
    public List<TransactionSplit> TransactionSplits { get; set; } = [];
    public List<BudgetAccount> BudgetAccounts { get; set; } = [];
    public List<BudgetMonth> BudgetMonths { get; set; } = [];
    public List<BudgetExpense> BudgetExpenses { get; set; } = [];
    public List<BudgetIncome> BudgetIncomes { get; set; } = [];
}
