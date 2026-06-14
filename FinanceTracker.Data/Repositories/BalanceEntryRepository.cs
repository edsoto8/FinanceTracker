using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class BalanceEntryRepository : IBalanceEntryRepository
{
    private readonly string _connectionString;

    public BalanceEntryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<IEnumerable<BalanceEntry>> GetByAccountAsync(int accountId)
    {
        using var conn = Open();
        return await conn.QueryAsync<BalanceEntry>(
            "SELECT * FROM BalanceEntries WHERE AccountId = @AccountId ORDER BY BalanceDate DESC",
            new { AccountId = accountId });
    }

    public async Task<BalanceEntry?> GetLatestByAccountAsync(int accountId)
    {
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<BalanceEntry>(
            "SELECT * FROM BalanceEntries WHERE AccountId = @AccountId ORDER BY BalanceDate DESC LIMIT 1",
            new { AccountId = accountId });
    }

    public async Task<int> AddAsync(BalanceEntry entry)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO BalanceEntries (AccountId, Amount, BalanceDate, Notes, CreatedAt)
            VALUES (@AccountId, @Amount, @BalanceDate, @Notes, @CreatedAt);
            SELECT last_insert_rowid();", entry);
    }
}
