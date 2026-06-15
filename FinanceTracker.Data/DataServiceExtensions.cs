using FinanceTracker.Core.Interfaces;
using FinanceTracker.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbPath = DatabasePathResolver.Resolve(configuration);

        services.AddSingleton(new SqliteConnectionFactory(dbPath));
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<CategorySeeder>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBalanceEntryRepository, BalanceEntryRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}
