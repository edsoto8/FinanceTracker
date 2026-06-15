using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetRecentAsync(int count);
    Task<Expense?> GetByIdAsync(int id);
    Task<int> InsertAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
}
