using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<int> InsertAsync(Category category);
    Task<bool> ExistsAsync(string name);
}
