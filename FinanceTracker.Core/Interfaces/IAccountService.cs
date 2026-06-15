using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAllAccountsAsync();
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
    Task<Account?> GetAccountByIdAsync(int id);
    Task<Account> AddAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeactivateAccountAsync(int id);
    Task DeleteAccountAsync(int id);
    Task<decimal> GetTotalBalanceAsync();
}
