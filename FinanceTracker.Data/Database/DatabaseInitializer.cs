using Dapper;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Database;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Balance REAL NOT NULL DEFAULT 0,
                Institution TEXT NOT NULL DEFAULT '',
                Notes TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS BalanceEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId INTEGER NOT NULL,
                Amount REAL NOT NULL,
                BalanceDate TEXT NOT NULL,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Expenses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId INTEGER NOT NULL,
                Category TEXT NOT NULL,
                Description TEXT NOT NULL,
                Amount REAL NOT NULL,
                TransactionDate TEXT NOT NULL,
                PaymentMethod TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
            );

            CREATE TABLE IF NOT EXISTS Subscriptions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Cost REAL NOT NULL,
                Frequency INTEGER NOT NULL,
                NextBillingDate TEXT NOT NULL,
                Category TEXT,
                PaymentAccountId INTEGER,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS Documents (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                ExpirationDate TEXT NOT NULL,
                ReminderDate TEXT,
                Notes TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
        ");

        await SeedCategoriesAsync(connection);
    }

    private static async Task SeedCategoriesAsync(SqliteConnection connection)
    {
        var existing = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Categories");
        if (existing > 0) return;

        var categories = new[]
        {
            "Food", "Groceries", "Gas", "Bills", "Rent/Mortgage",
            "Entertainment", "Subscriptions", "Travel", "Shopping", "Other"
        };

        foreach (var name in categories)
            await connection.ExecuteAsync("INSERT INTO Categories (Name) VALUES (@Name)", new { Name = name });
    }
}
