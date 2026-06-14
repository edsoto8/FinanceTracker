using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<IEnumerable<Document>> GetActiveAsync();
    Task<IEnumerable<Document>> GetExpiredAsync();
    Task<IEnumerable<Document>> GetExpiringSoonAsync(int days);
    Task<IEnumerable<Document>> GetWithUpcomingRemindersAsync(int days);
    Task<Document?> GetByIdAsync(int id);
    Task<int> AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(int id);
}
