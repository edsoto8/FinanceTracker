using System.Data;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data;

public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string dbPath)
        => _connectionString = $"Data Source={dbPath}";

    public string ConnectionString => _connectionString;

    public IDbConnection Create() => new SqliteConnection(_connectionString);
}
