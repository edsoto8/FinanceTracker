using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly SqliteConnectionFactory _factory;

    public AccountRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Account>(
            "SELECT * FROM Accounts ORDER BY Name;");
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        using var connection = _factory.Create();
        return await connection.QuerySingleOrDefaultAsync<Account>(
            "SELECT * FROM Accounts WHERE Id = @id;", new { id });
    }

    public async Task<int> InsertAsync(Account account)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO Accounts (Name, Type, Balance, Institution, Notes, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Type, @Balance, @Institution, @Notes, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();
            """, account);
    }

    public async Task UpdateAsync(Account account)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync(
            """
            UPDATE Accounts
            SET Name = @Name, Type = @Type, Balance = @Balance, Institution = @Institution,
                Notes = @Notes, IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """, account);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync("DELETE FROM Accounts WHERE Id = @id;", new { id });
    }
}
