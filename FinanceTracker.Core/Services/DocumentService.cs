using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Services;

public class DocumentService
{
    private readonly IDocumentRepository _documents;

    public DocumentService(IDocumentRepository documents)
    {
        _documents = documents;
    }

    public Task<IEnumerable<Document>> GetActiveAsync() => _documents.GetActiveAsync();
    public Task<IEnumerable<Document>> GetExpiredAsync() => _documents.GetExpiredAsync();
    public Task<IEnumerable<Document>> GetExpiringSoonAsync(int days = 30) =>
        _documents.GetExpiringSoonAsync(days);
    public Task<IEnumerable<Document>> GetWithUpcomingRemindersAsync(int days = 7) =>
        _documents.GetWithUpcomingRemindersAsync(days);

    public async Task<Document> AddDocumentAsync(Document document)
    {
        document.Id = await _documents.AddAsync(document);
        return document;
    }

    public Task UpdateDocumentAsync(Document document) => _documents.UpdateAsync(document);
    public Task DeleteDocumentAsync(int id) => _documents.DeleteAsync(id);

    public async Task DeactivateAsync(int id)
    {
        var doc = await _documents.GetByIdAsync(id);
        if (doc == null) return;
        doc.IsActive = false;
        await _documents.UpdateAsync(doc);
    }
}
