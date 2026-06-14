using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Services;

public class ExpenseService
{
    private readonly IExpenseRepository _expenses;

    public ExpenseService(IExpenseRepository expenses)
    {
        _expenses = expenses;
    }

    public Task<IEnumerable<Expense>> GetAllAsync() => _expenses.GetAllAsync();
    public Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        _expenses.GetByDateRangeAsync(from, to);
    public Task<IEnumerable<Expense>> GetByCategoryAsync(string category) =>
        _expenses.GetByCategoryAsync(category);

    public async Task<Expense> AddExpenseAsync(Expense expense)
    {
        expense.CreatedAt = DateTime.UtcNow;
        expense.Id = await _expenses.AddAsync(expense);
        return expense;
    }

    public Task UpdateExpenseAsync(Expense expense) => _expenses.UpdateAsync(expense);
    public Task DeleteExpenseAsync(int id) => _expenses.DeleteAsync(id);

    public Task<decimal> GetMonthlyTotalAsync(int year, int month) =>
        _expenses.GetMonthlyTotalAsync(year, month);

    public Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to) =>
        _expenses.GetSpendingByCategoryAsync(from, to);
}
