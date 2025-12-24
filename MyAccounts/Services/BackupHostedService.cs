namespace MyAccounts.Services;

public class BackupHostedService(IServiceScopeFactory scopeFactory, ILogger<BackupHostedService> logger) : BackgroundService
{
    // Run backup every Sunday at 2:00 AM
    private static readonly DayOfWeek BackupDay = DayOfWeek.Sunday;
    private static readonly TimeSpan BackupTime = new(2, 0, 0); // 2:00 AM

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Backup Hosted Service started. Scheduled for {Day} at {Time}", BackupDay, BackupTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = CalculateNextRun(now);
            var delay = nextRun - now;

            logger.LogInformation("Next scheduled backup at {NextRun} (in {Delay})", nextRun, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await RunBackupAsync();
            }
        }

        logger.LogInformation("Backup Hosted Service stopped");
    }

    private static DateTime CalculateNextRun(DateTime from)
    {
        var nextRun = from.Date.Add(BackupTime);

        // If we're past the backup time today, move to the next occurrence
        if (from.TimeOfDay >= BackupTime)
        {
            nextRun = nextRun.AddDays(1);
        }

        // Find the next occurrence of BackupDay
        while (nextRun.DayOfWeek != BackupDay)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }

    private async Task RunBackupAsync()
    {
        logger.LogInformation("Starting scheduled backup at {Time}", DateTime.Now);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();

            var filePath = await backupService.CreateScheduledBackupAsync();

            if (filePath != null)
            {
                logger.LogInformation("Scheduled backup completed successfully: {FilePath}", filePath);
            }
            else
            {
                logger.LogWarning("Scheduled backup completed but no file was created");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduled backup failed");
        }
    }
}
