using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IBalanceService
{
    Task<IEnumerable<BalanceEntry>> GetHistoryAsync(int accountId);
    Task<BalanceEntry> AddEntryAsync(BalanceEntry entry);
    Task<decimal> GetCurrentNetWorthAsync();
}
