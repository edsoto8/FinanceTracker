using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync();
    Task<IEnumerable<Account>> GetActiveAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<int> AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalBalanceAsync();
}
