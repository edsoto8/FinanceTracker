using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ISubscriptionRepository
{
    Task<IEnumerable<Subscription>> GetAllAsync();
    Task<int> InsertAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task DeleteAsync(int id);
}
