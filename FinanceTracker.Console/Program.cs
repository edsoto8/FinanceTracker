using FinanceTracker.Console.Menus;
using FinanceTracker.Core;
using FinanceTracker.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Serilog writes to file only; Spectre.Console owns the terminal output.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting FinanceTracker console application");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices(services =>
        {
            services.AddDataServices(configuration);
            services.AddCoreServices();

            services.AddTransient<MainMenu>();
            services.AddTransient<AccountMenu>();
            services.AddTransient<ExpenseMenu>();
            services.AddTransient<SubscriptionMenu>();
            services.AddTransient<DocumentMenu>();
            services.AddTransient<DashboardCommand>();
        })
        .Build();

    using var scope = host.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
    await scope.ServiceProvider.GetRequiredService<CategorySeeder>().SeedAsync();

    var mainMenu = scope.ServiceProvider.GetRequiredService<MainMenu>();
    await mainMenu.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FinanceTracker console application terminated unexpectedly");
    AnsiConsole.MarkupLine($"[red]Fatal error:[/] {Markup.Escape(ex.Message)}");
}
finally
{
    Log.CloseAndFlush();
}
