using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinanceTracker.Tests;

public class SubscriptionServiceTests
{
    private static SubscriptionService CreateService(TestDatabase db)
        => new(db.Subscriptions, NullLogger<SubscriptionService>.Instance);

    private static async Task<int> SeedCategoryAsync(TestDatabase db)
    {
        await db.SeedCategoriesAsync();
        return (await db.Categories.GetAllAsync()).First().Id;
    }

    [Fact]
    public async Task GetMonthlyEstimate_NormalizesAllFrequencies()
    {
        using var db = new TestDatabase();
        var categoryId = await SeedCategoryAsync(db);
        var service = CreateService(db);

        await service.AddAsync(new Subscription
        {
            Name = "Weekly",
            Cost = 10m,
            Frequency = BillingFrequency.Weekly,
            NextBillingDate = DateTime.Today,
            CategoryId = categoryId
        });
        await service.AddAsync(new Subscription
        {
            Name = "Monthly",
            Cost = 15m,
            Frequency = BillingFrequency.Monthly,
            NextBillingDate = DateTime.Today,
            CategoryId = categoryId
        });
        await service.AddAsync(new Subscription
        {
            Name = "Quarterly",
            Cost = 30m,
            Frequency = BillingFrequency.Quarterly,
            NextBillingDate = DateTime.Today,
            CategoryId = categoryId
        });
        await service.AddAsync(new Subscription
        {
            Name = "Yearly",
            Cost = 120m,
            Frequency = BillingFrequency.Yearly,
            NextBillingDate = DateTime.Today,
            CategoryId = categoryId
        });

        var estimate = await service.GetMonthlyEstimateAsync();

        // 10*4.33 + 15 + 30/3 + 120/12 = 43.3 + 15 + 10 + 10 = 78.3
        Assert.Equal(78.3m, estimate);
    }

    [Fact]
    public async Task GetMonthlyEstimate_ExcludesInactive()
    {
        using var db = new TestDatabase();
        var categoryId = await SeedCategoryAsync(db);
        var service = CreateService(db);

        var sub = await service.AddAsync(new Subscription
        {
            Name = "Gym",
            Cost = 50m,
            Frequency = BillingFrequency.Monthly,
            NextBillingDate = DateTime.Today,
            CategoryId = categoryId
        });
        await service.DeactivateAsync(sub.Id);

        var estimate = await service.GetMonthlyEstimateAsync();

        Assert.Equal(0m, estimate);
    }

    [Fact]
    public async Task GetUpcoming_RespectsDaysWindow()
    {
        using var db = new TestDatabase();
        var categoryId = await SeedCategoryAsync(db);
        var service = CreateService(db);

        await service.AddAsync(new Subscription
        {
            Name = "Soon",
            Cost = 5m,
            Frequency = BillingFrequency.Monthly,
            NextBillingDate = DateTime.Today.AddDays(3),
            CategoryId = categoryId
        });
        await service.AddAsync(new Subscription
        {
            Name = "Later",
            Cost = 5m,
            Frequency = BillingFrequency.Monthly,
            NextBillingDate = DateTime.Today.AddDays(20),
            CategoryId = categoryId
        });

        var upcoming = (await service.GetUpcomingAsync(7)).ToList();

        Assert.Single(upcoming);
        Assert.Equal("Soon", upcoming[0].Name);
    }
}
