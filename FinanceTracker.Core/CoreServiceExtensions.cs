using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Core;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IBalanceService, BalanceService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }
}
