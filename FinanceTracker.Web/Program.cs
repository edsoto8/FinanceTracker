using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Database;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Web.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting FinanceTracker.Web");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=financetracker.db";

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddSingleton<IAccountRepository>(_ => new AccountRepository(connectionString));
    builder.Services.AddSingleton<IBalanceEntryRepository>(_ => new BalanceEntryRepository(connectionString));
    builder.Services.AddSingleton<IExpenseRepository>(_ => new ExpenseRepository(connectionString));
    builder.Services.AddSingleton<ISubscriptionRepository>(_ => new SubscriptionRepository(connectionString));
    builder.Services.AddSingleton<IDocumentRepository>(_ => new DocumentRepository(connectionString));
    builder.Services.AddSingleton<ICategoryRepository>(_ => new CategoryRepository(connectionString));

    builder.Services.AddScoped<AccountService>();
    builder.Services.AddScoped<ExpenseService>();
    builder.Services.AddScoped<SubscriptionService>();
    builder.Services.AddScoped<DocumentService>();

    var app = builder.Build();

    var dbInit = new DatabaseInitializer(connectionString);
    await dbInit.InitializeAsync();
    Log.Information("Database initialized");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
