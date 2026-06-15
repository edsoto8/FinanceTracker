using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ISubscriptionRepository repository, ILogger<SubscriptionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<IEnumerable<Subscription>> GetActiveAsync()
    {
        var subscriptions = await _repository.GetAllAsync();
        return subscriptions.Where(s => s.IsActive);
    }

    public async Task<Subscription?> GetByIdAsync(int id)
    {
        var subscriptions = await _repository.GetAllAsync();
        return subscriptions.FirstOrDefault(s => s.Id == id);
    }

    public async Task<Subscription> AddAsync(Subscription subscription)
    {
        subscription.IsActive = true;
        subscription.Id = await _repository.InsertAsync(subscription);
        _logger.LogInformation(
            "Subscription added: {Name} ({Cost} {Frequency})",
            subscription.Name, subscription.Cost, subscription.Frequency);
        return subscription;
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        await _repository.UpdateAsync(subscription);
        _logger.LogInformation("Subscription updated: Id {SubscriptionId}", subscription.Id);
    }

    public async Task DeactivateAsync(int id)
    {
        var subscriptions = await _repository.GetAllAsync();
        var subscription = subscriptions.FirstOrDefault(s => s.Id == id);
        if (subscription is null)
        {
            return;
        }

        subscription.IsActive = false;
        await _repository.UpdateAsync(subscription);
        _logger.LogInformation("Subscription deactivated: Id {SubscriptionId}", id);
    }

    public async Task<decimal> GetMonthlyEstimateAsync()
    {
        var subscriptions = await _repository.GetAllAsync();
        return subscriptions
            .Where(s => s.IsActive)
            .Sum(s => ToMonthlyCost(s.Cost, s.Frequency));
    }

    public async Task<IEnumerable<Subscription>> GetUpcomingAsync(int daysAhead)
    {
        var subscriptions = await _repository.GetAllAsync();
        var cutoff = DateTime.Today.AddDays(daysAhead);
        return subscriptions
            .Where(s => s.IsActive && s.NextBillingDate.Date <= cutoff)
            .OrderBy(s => s.NextBillingDate);
    }

    private static decimal ToMonthlyCost(decimal cost, BillingFrequency frequency) => frequency switch
    {
        BillingFrequency.Weekly => cost * 4.33m,
        BillingFrequency.Monthly => cost,
        BillingFrequency.Quarterly => cost / 3m,
        BillingFrequency.Yearly => cost / 12m,
        _ => cost
    };
}
