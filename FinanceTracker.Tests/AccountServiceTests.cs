using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task AddAccount_CreatesActiveAccount()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new AccountService(new AccountRepository(cs), new BalanceEntryRepository(cs));

        var account = await service.AddAccountAsync(new Account
        {
            Name = "Checking", Institution = "Bank", Type = AccountType.Checking, Balance = 1000
        });

        Assert.True(account.Id > 0);
        Assert.True(account.IsActive);
        Assert.Equal("Checking", account.Name);
    }

    [Fact]
    public async Task GetTotalBalance_SumsActiveAccountBalances()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new AccountService(new AccountRepository(cs), new BalanceEntryRepository(cs));

        await service.AddAccountAsync(new Account { Name = "A", Institution = "B1", Type = AccountType.Checking, Balance = 1000 });
        await service.AddAccountAsync(new Account { Name = "B", Institution = "B2", Type = AccountType.Savings, Balance = 500 });

        var total = await service.GetTotalBalanceAsync();

        Assert.Equal(1500m, total);
    }

    [Fact]
    public async Task RecordBalance_UpdatesAccountBalance()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new AccountService(new AccountRepository(cs), new BalanceEntryRepository(cs));
        var account = await service.AddAccountAsync(new Account { Name = "A", Institution = "B", Type = AccountType.Checking, Balance = 500 });

        await service.RecordBalanceAsync(account.Id, 750);

        var updated = await service.GetAccountAsync(account.Id);
        Assert.Equal(750m, updated!.Balance);
    }

    [Fact]
    public async Task GetBalanceHistory_ReturnsEntriesForAccount()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new AccountService(new AccountRepository(cs), new BalanceEntryRepository(cs));
        var account = await service.AddAccountAsync(new Account { Name = "A", Institution = "B", Type = AccountType.Checking, Balance = 0 });

        await service.RecordBalanceAsync(account.Id, 100);
        await service.RecordBalanceAsync(account.Id, 200);

        var history = (await service.GetBalanceHistoryAsync(account.Id)).ToList();
        Assert.Equal(2, history.Count);
    }
}
