using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task SeedDefaultsAsync();
}
