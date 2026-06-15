using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SqliteConnectionFactory _factory;

    public SubscriptionRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Subscription>> GetAllAsync()
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Subscription>(
            "SELECT * FROM Subscriptions ORDER BY NextBillingDate;");
    }

    public async Task<int> InsertAsync(Subscription subscription)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO Subscriptions (Name, Cost, Frequency, NextBillingDate, CategoryId, AccountId, IsActive, Notes)
            VALUES (@Name, @Cost, @Frequency, @NextBillingDate, @CategoryId, @AccountId, @IsActive, @Notes);
            SELECT last_insert_rowid();
            """, subscription);
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync(
            """
            UPDATE Subscriptions
            SET Name = @Name, Cost = @Cost, Frequency = @Frequency, NextBillingDate = @NextBillingDate,
                CategoryId = @CategoryId, AccountId = @AccountId, IsActive = @IsActive, Notes = @Notes
            WHERE Id = @Id;
            """, subscription);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync("DELETE FROM Subscriptions WHERE Id = @id;", new { id });
    }
}
