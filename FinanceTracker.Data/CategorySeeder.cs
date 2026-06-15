using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Data;

public class CategorySeeder
{
    private static readonly string[] Defaults =
    {
        "Food", "Groceries", "Gas", "Bills", "Rent/Mortgage",
        "Entertainment", "Subscriptions", "Travel", "Shopping", "Other"
    };

    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategorySeeder>? _logger;

    public CategorySeeder(ICategoryRepository repository, ILogger<CategorySeeder>? logger = null)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var inserted = 0;
        foreach (var name in Defaults)
        {
            if (await _repository.ExistsAsync(name))
            {
                continue;
            }

            await _repository.InsertAsync(new Category { Name = name, IsDefault = true });
            inserted++;
        }

        if (inserted > 0)
        {
            _logger?.LogInformation("Seeded {Count} default categories", inserted);
        }
    }
}
