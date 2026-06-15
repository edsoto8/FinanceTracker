using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinanceTracker.Tests;

public class ExpenseServiceTests
{
    private static ExpenseService CreateService(TestDatabase db)
        => new(db.Expenses, db.Categories, NullLogger<ExpenseService>.Instance);

    [Fact]
    public async Task GetMonthlyTotal_SumsOnlyTargetMonth()
    {
        using var db = new TestDatabase();
        await db.SeedCategoriesAsync();
        var categoryId = (await db.Categories.GetAllAsync()).First().Id;
        var service = CreateService(db);

        await service.AddExpenseAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "March 1",
            Amount = 30m,
            TransactionDate = new DateTime(2026, 3, 5)
        });
        await service.AddExpenseAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "March 2",
            Amount = 20m,
            TransactionDate = new DateTime(2026, 3, 28)
        });
        await service.AddExpenseAsync(new Expense
        {
            CategoryId = categoryId,
            Description = "April",
            Amount = 99m,
            TransactionDate = new DateTime(2026, 4, 1)
        });

        var total = await service.GetMonthlyTotalAsync(2026, 3);

        Assert.Equal(50m, total);
    }

    [Fact]
    public async Task GetSpendingByCategory_GroupsByCategoryName()
    {
        using var db = new TestDatabase();
        await db.SeedCategoriesAsync();
        var categories = (await db.Categories.GetAllAsync()).ToList();
        var food = categories.First(c => c.Name == "Food").Id;
        var gas = categories.First(c => c.Name == "Gas").Id;
        var service = CreateService(db);

        await service.AddExpenseAsync(new Expense
        {
            CategoryId = food,
            Description = "Lunch",
            Amount = 12m,
            TransactionDate = new DateTime(2026, 6, 1)
        });
        await service.AddExpenseAsync(new Expense
        {
            CategoryId = food,
            Description = "Dinner",
            Amount = 18m,
            TransactionDate = new DateTime(2026, 6, 2)
        });
        await service.AddExpenseAsync(new Expense
        {
            CategoryId = gas,
            Description = "Fuel",
            Amount = 40m,
            TransactionDate = new DateTime(2026, 6, 3)
        });

        var breakdown = await service.GetSpendingByCategoryAsync(
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 30));

        Assert.Equal(30m, breakdown["Food"]);
        Assert.Equal(40m, breakdown["Gas"]);
    }
}
