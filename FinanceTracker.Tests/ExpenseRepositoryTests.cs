using FinanceTracker.Core.Models;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class ExpenseRepositoryTests
{
    [Fact]
    public async Task GetByDateRange_FiltersCorrectly()
    {
        using var db = new TestDatabase();
        await db.SeedCategoriesAsync();
        var categoryId = (await db.Categories.GetAllAsync()).First().Id;
        var repo = db.Expenses;

        await repo.InsertAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "In range",
            Amount = 10m,
            TransactionDate = new DateTime(2026, 5, 15),
            CreatedAt = DateTime.Now
        });
        await repo.InsertAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "Out of range",
            Amount = 99m,
            TransactionDate = new DateTime(2026, 7, 1),
            CreatedAt = DateTime.Now
        });

        var results = (await repo.GetByDateRangeAsync(
            new DateTime(2026, 5, 1), new DateTime(2026, 5, 31))).ToList();

        Assert.Single(results);
        Assert.Equal("In range", results[0].Description);
    }

    [Fact]
    public async Task GetRecent_OrdersByDateDescending()
    {
        using var db = new TestDatabase();
        await db.SeedCategoriesAsync();
        var categoryId = (await db.Categories.GetAllAsync()).First().Id;
        var repo = db.Expenses;

        await repo.InsertAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "Older",
            Amount = 10m,
            TransactionDate = new DateTime(2026, 1, 1),
            CreatedAt = DateTime.Now
        });
        await repo.InsertAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "Newer",
            Amount = 20m,
            TransactionDate = new DateTime(2026, 6, 1),
            CreatedAt = DateTime.Now
        });

        var results = (await repo.GetRecentAsync(1)).ToList();

        Assert.Single(results);
        Assert.Equal("Newer", results[0].Description);
    }
}
