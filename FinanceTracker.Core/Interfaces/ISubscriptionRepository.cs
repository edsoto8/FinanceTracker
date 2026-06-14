using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ISubscriptionRepository
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<IEnumerable<Subscription>> GetActiveAsync();
    Task<IEnumerable<Subscription>> GetUpcomingAsync(int days);
    Task<Subscription?> GetByIdAsync(int id);
    Task<int> AddAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task<decimal> GetEstimatedMonthlyTotalAsync();
}
