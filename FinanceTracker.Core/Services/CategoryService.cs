using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class CategoryService : ICategoryService
{
    private static readonly string[] DefaultCategories =
    {
        "Food", "Groceries", "Gas", "Bills", "Rent/Mortgage",
        "Entertainment", "Subscriptions", "Travel", "Shopping", "Other"
    };

    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Category> AddAsync(Category category)
    {
        category.Id = await _repository.InsertAsync(category);
        _logger.LogInformation("Category added: {Name}", category.Name);
        return category;
    }

    public async Task SeedDefaultsAsync()
    {
        var inserted = 0;
        foreach (var name in DefaultCategories)
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
            _logger.LogInformation("Seeded {Count} default categories", inserted);
        }
    }
}
