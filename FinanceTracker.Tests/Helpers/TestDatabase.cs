using FinanceTracker.Data;
using FinanceTracker.Data.Repositories;

namespace FinanceTracker.Tests.Helpers;

/// <summary>
/// Creates an isolated temp-file SQLite database with the full schema applied.
/// Dispose deletes the file.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly string _path;

    public TestDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"financetracker-test-{Guid.NewGuid():N}.db");
        Factory = new SqliteConnectionFactory(_path);
        new DatabaseInitializer(Factory).Initialize();
    }

    public SqliteConnectionFactory Factory { get; }

    public AccountRepository Accounts => new(Factory);
    public BalanceEntryRepository BalanceEntries => new(Factory);
    public ExpenseRepository Expenses => new(Factory);
    public SubscriptionRepository Subscriptions => new(Factory);
    public DocumentRepository Documents => new(Factory);
    public CategoryRepository Categories => new(Factory);

    public async Task SeedCategoriesAsync()
        => await new CategorySeeder(Categories).SeedAsync();

    public void Dispose()
    {
        // Clear pooled connections so the file is no longer locked, then delete.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
