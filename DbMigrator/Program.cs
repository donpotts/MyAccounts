using DbMigrator;
using Microsoft.EntityFrameworkCore;
using MyAccounts.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<OldDbContext>(options =>
            options.UseSqlite(context.Configuration.GetConnectionString("OldDbContext")).EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("ApplicationDbContext")));

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
