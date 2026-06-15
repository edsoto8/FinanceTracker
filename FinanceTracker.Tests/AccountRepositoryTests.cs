using FinanceTracker.Core.Models;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class AccountRepositoryTests
{
    [Fact]
    public async Task Insert_ReturnsNewId_AndPersists()
    {
        using var db = new TestDatabase();
        var repo = db.Accounts;

        var id = await repo.InsertAsync(new Account
        {
            Name = "Savings",
            Type = AccountType.Savings,
            Balance = 500m,
            Institution = "Bank",
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        Assert.True(id > 0);
        var stored = await repo.GetByIdAsync(id);
        Assert.NotNull(stored);
        Assert.Equal("Savings", stored!.Name);
        Assert.Equal(AccountType.Savings, stored.Type);
        Assert.Equal(500m, stored.Balance);
    }

    [Fact]
    public async Task GetById_ReturnsNull_ForMissing()
    {
        using var db = new TestDatabase();

        var result = await db.Accounts.GetByIdAsync(999);

        Assert.Null(result);
    }
}
