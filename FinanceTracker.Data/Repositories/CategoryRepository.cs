using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        return await conn.QueryAsync<Category>("SELECT * FROM Categories ORDER BY Name");
    }
}
