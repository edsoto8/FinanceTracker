using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly SqliteConnectionFactory _factory;

    public CategoryRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Category>(
            "SELECT * FROM Categories ORDER BY Name;");
    }

    public async Task<int> InsertAsync(Category category)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO Categories (Name, IsDefault)
            VALUES (@Name, @IsDefault);
            SELECT last_insert_rowid();
            """, category);
    }

    public async Task<bool> ExistsAsync(string name)
    {
        using var connection = _factory.Create();
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Categories WHERE Name = @name;", new { name });
        return count > 0;
    }
}
