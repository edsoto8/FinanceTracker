using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly string _connectionString;

    public SubscriptionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Subscription>("SELECT * FROM Subscriptions ORDER BY Name");
    }

    public async Task<IEnumerable<Subscription>> GetActiveAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Subscription>(
            "SELECT * FROM Subscriptions WHERE IsActive = 1 ORDER BY NextBillingDate");
    }

    public async Task<IEnumerable<Subscription>> GetUpcomingAsync(int days)
    {
        using var conn = Open();
        var cutoff = DateTime.UtcNow.AddDays(days);
        return await conn.QueryAsync<Subscription>(
            "SELECT * FROM Subscriptions WHERE IsActive = 1 AND NextBillingDate <= @Cutoff ORDER BY NextBillingDate",
            new { Cutoff = cutoff });
    }

    public async Task<Subscription?> GetByIdAsync(int id)
    {
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<Subscription>(
            "SELECT * FROM Subscriptions WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(Subscription subscription)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Subscriptions (Name, Cost, Frequency, NextBillingDate, Category, PaymentAccountId, IsActive, Notes)
            VALUES (@Name, @Cost, @Frequency, @NextBillingDate, @Category, @PaymentAccountId, @IsActive, @Notes);
            SELECT last_insert_rowid();", subscription);
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        using var conn = Open();
        await conn.ExecuteAsync(@"
            UPDATE Subscriptions SET Name=@Name, Cost=@Cost, Frequency=@Frequency, NextBillingDate=@NextBillingDate,
            Category=@Category, PaymentAccountId=@PaymentAccountId, IsActive=@IsActive, Notes=@Notes
            WHERE Id=@Id", subscription);
    }

    public async Task<decimal> GetEstimatedMonthlyTotalAsync()
    {
        var active = await GetActiveAsync();
        return active.Sum(s => s.MonthlyCost);
    }
}
