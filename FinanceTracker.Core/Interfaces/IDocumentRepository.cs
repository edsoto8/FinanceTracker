using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(int id);
    Task<int> InsertAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(int id);
}
