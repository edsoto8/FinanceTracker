using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly SqliteConnectionFactory _factory;

    public ExpenseRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Expense>(
            """
            SELECT * FROM Expenses
            WHERE TransactionDate >= @from AND TransactionDate <= @to
            ORDER BY TransactionDate DESC;
            """, new { from, to });
    }

    public async Task<IEnumerable<Expense>> GetRecentAsync(int count)
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Expense>(
            "SELECT * FROM Expenses ORDER BY TransactionDate DESC, Id DESC LIMIT @count;",
            new { count });
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        using var connection = _factory.Create();
        return await connection.QuerySingleOrDefaultAsync<Expense>(
            "SELECT * FROM Expenses WHERE Id = @id;", new { id });
    }

    public async Task<int> InsertAsync(Expense expense)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO Expenses (AccountId, CategoryId, Description, Amount, TransactionDate, PaymentMethod, Notes, CreatedAt)
            VALUES (@AccountId, @CategoryId, @Description, @Amount, @TransactionDate, @PaymentMethod, @Notes, @CreatedAt);
            SELECT last_insert_rowid();
            """, expense);
    }

    public async Task UpdateAsync(Expense expense)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET AccountId = @AccountId, CategoryId = @CategoryId, Description = @Description,
                Amount = @Amount, TransactionDate = @TransactionDate, PaymentMethod = @PaymentMethod,
                Notes = @Notes
            WHERE Id = @Id;
            """, expense);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync("DELETE FROM Expenses WHERE Id = @id;", new { id });
    }
}
