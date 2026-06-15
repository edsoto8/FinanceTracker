using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountRepository repository, ILogger<AccountService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        => await _repository.GetAllAsync();

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
    {
        var accounts = await _repository.GetAllAsync();
        return accounts.Where(a => a.IsActive);
    }

    public async Task<Account?> GetAccountByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<Account> AddAccountAsync(Account account)
    {
        account.IsActive = true;
        account.CreatedAt = DateTime.Now;
        account.UpdatedAt = DateTime.Now;
        account.Id = await _repository.InsertAsync(account);
        _logger.LogInformation("Account added: {AccountName} (Id {AccountId})", account.Name, account.Id);
        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        account.UpdatedAt = DateTime.Now;
        await _repository.UpdateAsync(account);
        _logger.LogInformation("Account updated: Id {AccountId}", account.Id);
    }

    public async Task DeactivateAccountAsync(int id)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account is null)
        {
            return;
        }

        account.IsActive = false;
        account.UpdatedAt = DateTime.Now;
        await _repository.UpdateAsync(account);
        _logger.LogInformation("Account deactivated: Id {AccountId}", id);
    }

    public async Task DeleteAccountAsync(int id)
    {
        await _repository.DeleteAsync(id);
        _logger.LogWarning("Account deleted: Id {AccountId}", id);
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        var accounts = await _repository.GetAllAsync();
        return accounts.Where(a => a.IsActive).Sum(a => a.Balance);
    }
}
