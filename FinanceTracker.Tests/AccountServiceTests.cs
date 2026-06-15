using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinanceTracker.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task AddAccount_CreatesActiveAccount()
    {
        using var db = new TestDatabase();
        var service = new AccountService(db.Accounts, NullLogger<AccountService>.Instance);

        var result = await service.AddAccountAsync(new Account
        {
            Name = "Checking",
            Type = AccountType.Checking,
            Balance = 100m,
            IsActive = false // service should force this true
        });

        Assert.True(result.Id > 0);
        var stored = await service.GetAccountByIdAsync(result.Id);
        Assert.NotNull(stored);
        Assert.True(stored!.IsActive);
        Assert.Equal("Checking", stored.Name);
    }

    [Fact]
    public async Task GetTotalBalance_SumsOnlyActiveAccounts()
    {
        using var db = new TestDatabase();
        var service = new AccountService(db.Accounts, NullLogger<AccountService>.Instance);

        await service.AddAccountAsync(new Account { Name = "A", Type = AccountType.Savings, Balance = 200m });
        await service.AddAccountAsync(new Account { Name = "B", Type = AccountType.Checking, Balance = 50m });
        var inactive = await service.AddAccountAsync(
            new Account { Name = "C", Type = AccountType.Other, Balance = 1000m });
        await service.DeactivateAccountAsync(inactive.Id);

        var total = await service.GetTotalBalanceAsync();

        Assert.Equal(250m, total);
    }
}
