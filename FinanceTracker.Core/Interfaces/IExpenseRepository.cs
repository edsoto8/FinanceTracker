using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetAllAsync();
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Expense>> GetByCategoryAsync(string category);
    Task<Expense?> GetByIdAsync(int id);
    Task<int> AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
    Task<decimal> GetMonthlyTotalAsync(int year, int month);
    Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to);
}
