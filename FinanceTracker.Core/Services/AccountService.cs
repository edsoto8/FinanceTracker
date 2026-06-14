using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Services;

public class AccountService
{
    private readonly IAccountRepository _accounts;
    private readonly IBalanceEntryRepository _balanceEntries;

    public AccountService(IAccountRepository accounts, IBalanceEntryRepository balanceEntries)
    {
        _accounts = accounts;
        _balanceEntries = balanceEntries;
    }

    public Task<IEnumerable<Account>> GetAllAccountsAsync() => _accounts.GetAllAsync();
    public Task<IEnumerable<Account>> GetActiveAccountsAsync() => _accounts.GetActiveAsync();
    public Task<Account?> GetAccountAsync(int id) => _accounts.GetByIdAsync(id);
    public Task<decimal> GetTotalBalanceAsync() => _accounts.GetTotalBalanceAsync();

    public async Task<Account> AddAccountAsync(Account account)
    {
        account.CreatedAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        account.Id = await _accounts.AddAsync(account);
        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        account.UpdatedAt = DateTime.UtcNow;
        await _accounts.UpdateAsync(account);
    }

    public Task DeleteAccountAsync(int id) => _accounts.DeleteAsync(id);

    public async Task RecordBalanceAsync(int accountId, decimal amount, string? notes = null)
    {
        var entry = new BalanceEntry
        {
            AccountId = accountId,
            Amount = amount,
            BalanceDate = DateTime.UtcNow,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
        await _balanceEntries.AddAsync(entry);

        var account = await _accounts.GetByIdAsync(accountId);
        if (account != null)
        {
            account.Balance = amount;
            await _accounts.UpdateAsync(account);
        }
    }

    public Task<IEnumerable<BalanceEntry>> GetBalanceHistoryAsync(int accountId) =>
        _balanceEntries.GetByAccountAsync(accountId);
}
