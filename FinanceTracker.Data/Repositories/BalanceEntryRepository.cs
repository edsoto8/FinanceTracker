using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class BalanceEntryRepository : IBalanceEntryRepository
{
    private readonly SqliteConnectionFactory _factory;

    public BalanceEntryRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<BalanceEntry>> GetByAccountIdAsync(int accountId)
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<BalanceEntry>(
            "SELECT * FROM BalanceEntries WHERE AccountId = @accountId ORDER BY EntryDate;",
            new { accountId });
    }

    public async Task<int> InsertAsync(BalanceEntry entry)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO BalanceEntries (AccountId, Amount, EntryDate, Notes, CreatedAt)
            VALUES (@AccountId, @Amount, @EntryDate, @Notes, @CreatedAt);
            SELECT last_insert_rowid();
            """, entry);
    }
}
