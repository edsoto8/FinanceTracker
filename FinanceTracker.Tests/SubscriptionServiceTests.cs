using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class SubscriptionServiceTests
{
    [Fact]
    public async Task MonthlyCost_CalculatesCorrectlyForEachFrequency()
    {
        var monthly = new Subscription { Cost = 10, Frequency = BillingFrequency.Monthly };
        var yearly = new Subscription { Cost = 120, Frequency = BillingFrequency.Yearly };
        var quarterly = new Subscription { Cost = 30, Frequency = BillingFrequency.Quarterly };
        var weekly = new Subscription { Cost = 10, Frequency = BillingFrequency.Weekly };

        Assert.Equal(10m, monthly.MonthlyCost);
        Assert.Equal(10m, yearly.MonthlyCost);
        Assert.Equal(10m, quarterly.MonthlyCost);
        Assert.Equal(10m * 52 / 12, weekly.MonthlyCost);
    }

    [Fact]
    public async Task GetEstimatedMonthlyTotal_SumsActiveSubscriptions()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new SubscriptionService(new SubscriptionRepository(cs));

        await service.AddSubscriptionAsync(new Subscription { Name = "Netflix", Cost = 15, Frequency = BillingFrequency.Monthly, NextBillingDate = DateTime.Today.AddDays(10) });
        await service.AddSubscriptionAsync(new Subscription { Name = "Annual", Cost = 120, Frequency = BillingFrequency.Yearly, NextBillingDate = DateTime.Today.AddDays(30) });

        var total = await service.GetEstimatedMonthlyTotalAsync();

        Assert.Equal(25m, total);
    }

    [Fact]
    public async Task DeactivateAsync_SetsSubscriptionInactive()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new SubscriptionService(new SubscriptionRepository(cs));
        var sub = await service.AddSubscriptionAsync(new Subscription { Name = "Test", Cost = 10, Frequency = BillingFrequency.Monthly, NextBillingDate = DateTime.Today });

        await service.DeactivateAsync(sub.Id);

        var active = (await service.GetActiveAsync()).ToList();
        Assert.DoesNotContain(active, s => s.Id == sub.Id);
    }
}
