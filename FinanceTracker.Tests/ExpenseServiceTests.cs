using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class ExpenseServiceTests
{
    private static async Task<(ExpenseService, int accountId)> SetupAsync(string cs)
    {
        var accountService = new AccountService(new AccountRepository(cs), new BalanceEntryRepository(cs));
        var account = await accountService.AddAccountAsync(new Account { Name = "Test", Institution = "Bank", Type = AccountType.Checking, Balance = 0 });
        return (new ExpenseService(new ExpenseRepository(cs)), account.Id);
    }

    [Fact]
    public async Task GetMonthlyTotal_SumsExpensesInMonth()
    {
        var cs = await TestDatabase.CreateAsync();
        var (service, accountId) = await SetupAsync(cs);

        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Food", Description = "Lunch", Amount = 15, TransactionDate = new DateTime(2025, 3, 10) });
        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Food", Description = "Dinner", Amount = 30, TransactionDate = new DateTime(2025, 3, 20) });
        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Gas", Description = "Gas", Amount = 50, TransactionDate = new DateTime(2025, 4, 1) });

        var total = await service.GetMonthlyTotalAsync(2025, 3);

        Assert.Equal(45m, total);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsExpensesInRange()
    {
        var cs = await TestDatabase.CreateAsync();
        var (service, accountId) = await SetupAsync(cs);

        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Food", Description = "A", Amount = 10, TransactionDate = new DateTime(2025, 3, 1) });
        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Food", Description = "B", Amount = 20, TransactionDate = new DateTime(2025, 3, 15) });
        await service.AddExpenseAsync(new Expense { AccountId = accountId, Category = "Food", Description = "C", Amount = 30, TransactionDate = new DateTime(2025, 4, 1) });

        var results = (await service.GetByDateRangeAsync(new DateTime(2025, 3, 1), new DateTime(2025, 3, 31))).ToList();

        Assert.Equal(2, results.Count);
    }
}
