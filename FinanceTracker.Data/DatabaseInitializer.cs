using Dapper;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Data;

public class DatabaseInitializer
{
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS Categories (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Name      TEXT NOT NULL UNIQUE,
            IsDefault INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS Accounts (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            Name        TEXT NOT NULL,
            Type        INTEGER NOT NULL,
            Balance     REAL NOT NULL DEFAULT 0,
            Institution TEXT,
            Notes       TEXT,
            IsActive    INTEGER NOT NULL DEFAULT 1,
            CreatedAt   TEXT NOT NULL,
            UpdatedAt   TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS BalanceEntries (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            AccountId INTEGER NOT NULL REFERENCES Accounts(Id),
            Amount    REAL NOT NULL,
            EntryDate TEXT NOT NULL,
            Notes     TEXT,
            CreatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Expenses (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            AccountId       INTEGER REFERENCES Accounts(Id),
            CategoryId      INTEGER NOT NULL REFERENCES Categories(Id),
            Description     TEXT NOT NULL,
            Amount          REAL NOT NULL,
            TransactionDate TEXT NOT NULL,
            PaymentMethod   TEXT,
            Notes           TEXT,
            CreatedAt       TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Subscriptions (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            Name            TEXT NOT NULL,
            Cost            REAL NOT NULL,
            Frequency       INTEGER NOT NULL,
            NextBillingDate TEXT NOT NULL,
            CategoryId      INTEGER NOT NULL REFERENCES Categories(Id),
            AccountId       INTEGER REFERENCES Accounts(Id),
            IsActive        INTEGER NOT NULL DEFAULT 1,
            Notes           TEXT
        );

        CREATE TABLE IF NOT EXISTS Documents (
            Id             INTEGER PRIMARY KEY AUTOINCREMENT,
            Name           TEXT NOT NULL,
            Type           INTEGER NOT NULL,
            ExpirationDate TEXT NOT NULL,
            ReminderDate   TEXT,
            Notes          TEXT,
            IsActive       INTEGER NOT NULL DEFAULT 1,
            CreatedAt      TEXT NOT NULL,
            UpdatedAt      TEXT NOT NULL
        );
        """;

    private readonly SqliteConnectionFactory _factory;
    private readonly ILogger<DatabaseInitializer>? _logger;

    public DatabaseInitializer(SqliteConnectionFactory factory, ILogger<DatabaseInitializer>? logger = null)
    {
        _factory = factory;
        _logger = logger;
    }

    public void Initialize()
    {
        using var connection = _factory.Create();
        connection.Open();
        connection.Execute(Schema);
        _logger?.LogInformation("Database initialized at {ConnectionString}", _factory.ConnectionString);
    }
}
