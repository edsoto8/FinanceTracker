using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Services;

public class SubscriptionService
{
    private readonly ISubscriptionRepository _subscriptions;

    public SubscriptionService(ISubscriptionRepository subscriptions)
    {
        _subscriptions = subscriptions;
    }

    public Task<IEnumerable<Subscription>> GetActiveAsync() => _subscriptions.GetActiveAsync();
    public Task<IEnumerable<Subscription>> GetUpcomingAsync(int days = 30) =>
        _subscriptions.GetUpcomingAsync(days);
    public Task<decimal> GetEstimatedMonthlyTotalAsync() =>
        _subscriptions.GetEstimatedMonthlyTotalAsync();

    public async Task<Subscription> AddSubscriptionAsync(Subscription subscription)
    {
        subscription.Id = await _subscriptions.AddAsync(subscription);
        return subscription;
    }

    public Task UpdateSubscriptionAsync(Subscription subscription) =>
        _subscriptions.UpdateAsync(subscription);

    public async Task DeactivateAsync(int id)
    {
        var sub = await _subscriptions.GetByIdAsync(id);
        if (sub == null) return;
        sub.IsActive = false;
        await _subscriptions.UpdateAsync(sub);
    }
}
