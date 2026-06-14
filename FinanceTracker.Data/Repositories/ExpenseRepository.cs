using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly string _connectionString;

    public ExpenseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Expense>("SELECT * FROM Expenses ORDER BY TransactionDate DESC");
    }

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        using var conn = Open();
        return await conn.QueryAsync<Expense>(
            "SELECT * FROM Expenses WHERE TransactionDate >= @From AND TransactionDate <= @To ORDER BY TransactionDate DESC",
            new { From = from, To = to });
    }

    public async Task<IEnumerable<Expense>> GetByCategoryAsync(string category)
    {
        using var conn = Open();
        return await conn.QueryAsync<Expense>(
            "SELECT * FROM Expenses WHERE Category = @Category ORDER BY TransactionDate DESC",
            new { Category = category });
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<Expense>(
            "SELECT * FROM Expenses WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(Expense expense)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Expenses (AccountId, Category, Description, Amount, TransactionDate, PaymentMethod, Notes, CreatedAt)
            VALUES (@AccountId, @Category, @Description, @Amount, @TransactionDate, @PaymentMethod, @Notes, @CreatedAt);
            SELECT last_insert_rowid();", expense);
    }

    public async Task UpdateAsync(Expense expense)
    {
        using var conn = Open();
        await conn.ExecuteAsync(@"
            UPDATE Expenses SET AccountId=@AccountId, Category=@Category, Description=@Description,
            Amount=@Amount, TransactionDate=@TransactionDate, PaymentMethod=@PaymentMethod, Notes=@Notes
            WHERE Id=@Id", expense);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = Open();
        await conn.ExecuteAsync("DELETE FROM Expenses WHERE Id = @Id", new { Id = id });
    }

    public async Task<decimal> GetMonthlyTotalAsync(int year, int month)
    {
        using var conn = Open();
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        return await conn.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(Amount), 0) FROM Expenses WHERE TransactionDate >= @From AND TransactionDate <= @To",
            new { From = from, To = to });
    }

    public async Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to)
    {
        using var conn = Open();
        var rows = await conn.QueryAsync(
            "SELECT Category, SUM(Amount) AS Total FROM Expenses WHERE TransactionDate >= @From AND TransactionDate <= @To GROUP BY Category",
            new { From = from, To = to });
        return rows.ToDictionary(r => (string)r.Category, r => (decimal)r.Total);
    }
}
