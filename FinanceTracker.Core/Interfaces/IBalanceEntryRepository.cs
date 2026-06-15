using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IBalanceEntryRepository
{
    Task<IEnumerable<BalanceEntry>> GetByAccountIdAsync(int accountId);
    Task<int> InsertAsync(BalanceEntry entry);
}
