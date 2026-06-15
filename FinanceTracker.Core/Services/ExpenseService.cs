using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository repository,
        ICategoryRepository categoryRepository,
        ILogger<ExpenseService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Expense>> GetExpensesAsync(DateTime from, DateTime to)
        => await _repository.GetByDateRangeAsync(from, to);

    public async Task<IEnumerable<Expense>> GetRecentAsync(int count)
        => await _repository.GetRecentAsync(count);

    public async Task<Expense?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<Expense> AddExpenseAsync(Expense expense)
    {
        expense.CreatedAt = DateTime.Now;
        expense.Id = await _repository.InsertAsync(expense);
        _logger.LogInformation(
            "Expense added: {Description} ({Amount})", expense.Description, expense.Amount);
        return expense;
    }

    public async Task UpdateExpenseAsync(Expense expense)
    {
        await _repository.UpdateAsync(expense);
        _logger.LogInformation("Expense updated: Id {ExpenseId}", expense.Id);
    }

    public async Task DeleteExpenseAsync(int id)
    {
        await _repository.DeleteAsync(id);
        _logger.LogWarning("Expense deleted: Id {ExpenseId}", id);
    }

    public async Task<decimal> GetMonthlyTotalAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        var expenses = await _repository.GetByDateRangeAsync(from, to);
        return expenses.Sum(e => e.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(DateTime from, DateTime to)
    {
        var expenses = await _repository.GetByDateRangeAsync(from, to);
        var categories = (await _categoryRepository.GetAllAsync())
            .ToDictionary(c => c.Id, c => c.Name);

        return expenses
            .GroupBy(e => categories.TryGetValue(e.CategoryId, out var name) ? name : "Uncategorized")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }
}
