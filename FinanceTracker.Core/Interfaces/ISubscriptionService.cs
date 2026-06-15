using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<IEnumerable<Subscription>> GetActiveAsync();
    Task<Subscription?> GetByIdAsync(int id);
    Task<Subscription> AddAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task DeactivateAsync(int id);
    Task<decimal> GetMonthlyEstimateAsync();
    Task<IEnumerable<Subscription>> GetUpcomingAsync(int daysAhead);
}
