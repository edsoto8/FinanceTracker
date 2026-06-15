using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetExpensesAsync(DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetRecentAsync(int count);
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense> AddExpenseAsync(Expense expense);
    Task UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(int id);
    Task<decimal> GetMonthlyTotalAsync(int year, int month);
    Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to);
}
