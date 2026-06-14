using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
}
