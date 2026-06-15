using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<IEnumerable<Document>> GetActiveAsync();
    Task<Document?> GetByIdAsync(int id);
    Task<Document> AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
    Task<IEnumerable<Document>> GetExpiredAsync();
    Task<IEnumerable<Document>> GetExpiringSoonAsync(int daysAhead);
    Task<IEnumerable<Document>> GetDueForReminderAsync();
}
