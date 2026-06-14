using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Account>("SELECT * FROM Accounts ORDER BY Name");
    }

    public async Task<IEnumerable<Account>> GetActiveAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Account>("SELECT * FROM Accounts WHERE IsActive = 1 ORDER BY Name");
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<Account>(
            "SELECT * FROM Accounts WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(Account account)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Accounts (Name, Type, Balance, Institution, Notes, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Type, @Balance, @Institution, @Notes, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();", account);
    }

    public async Task UpdateAsync(Account account)
    {
        using var conn = Open();
        await conn.ExecuteAsync(@"
            UPDATE Accounts SET Name=@Name, Type=@Type, Balance=@Balance, Institution=@Institution,
            Notes=@Notes, IsActive=@IsActive, UpdatedAt=@UpdatedAt WHERE Id=@Id", account);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = Open();
        await conn.ExecuteAsync("DELETE FROM Accounts WHERE Id = @Id", new { Id = id });
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(Balance), 0) FROM Accounts WHERE IsActive = 1");
    }
}
