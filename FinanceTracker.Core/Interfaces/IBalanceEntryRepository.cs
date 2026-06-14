using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IBalanceEntryRepository
{
    Task<IEnumerable<BalanceEntry>> GetByAccountAsync(int accountId);
    Task<BalanceEntry?> GetLatestByAccountAsync(int accountId);
    Task<int> AddAsync(BalanceEntry entry);
}
