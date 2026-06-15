using FinanceTracker.Core.Models;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class BalanceEntryRepositoryTests
{
    [Fact]
    public async Task Insert_AndRetrieveByAccount()
    {
        using var db = new TestDatabase();
        var accountId = await db.Accounts.InsertAsync(new Account
        {
            Name = "Brokerage",
            Type = AccountType.Investment,
            Balance = 0m,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        var repo = db.BalanceEntries;

        await repo.InsertAsync(new BalanceEntry
        {
            AccountId = accountId,
            Amount = 1000m,
            EntryDate = new DateTime(2026, 1, 1),
            CreatedAt = DateTime.Now
        });
        await repo.InsertAsync(new BalanceEntry
        {
            AccountId = accountId,
            Amount = 1500m,
            EntryDate = new DateTime(2026, 2, 1),
            CreatedAt = DateTime.Now
        });

        var entries = (await repo.GetByAccountIdAsync(accountId)).ToList();

        Assert.Equal(2, entries.Count);
        Assert.Equal(1000m, entries[0].Amount);
        Assert.Equal(1500m, entries[1].Amount);
    }

    [Fact]
    public async Task GetByAccount_ReturnsEmpty_WhenNoEntries()
    {
        using var db = new TestDatabase();

        var entries = await db.BalanceEntries.GetByAccountIdAsync(42);

        Assert.Empty(entries);
    }
}
