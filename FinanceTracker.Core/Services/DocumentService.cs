using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentRepository repository, ILogger<DocumentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<IEnumerable<Document>> GetActiveAsync()
    {
        var documents = await _repository.GetAllAsync();
        return documents.Where(d => d.IsActive);
    }

    public async Task<Document?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<Document> AddAsync(Document document)
    {
        document.IsActive = true;
        document.CreatedAt = DateTime.Now;
        document.UpdatedAt = DateTime.Now;
        document.Id = await _repository.InsertAsync(document);
        _logger.LogInformation(
            "Document added: {Name} (expires {ExpirationDate:d})", document.Name, document.ExpirationDate);
        return document;
    }

    public async Task UpdateAsync(Document document)
    {
        document.UpdatedAt = DateTime.Now;
        await _repository.UpdateAsync(document);
        _logger.LogInformation("Document updated: Id {DocumentId}", document.Id);
    }

    public async Task DeactivateAsync(int id)
    {
        var document = await _repository.GetByIdAsync(id);
        if (document is null)
        {
            return;
        }

        document.IsActive = false;
        document.UpdatedAt = DateTime.Now;
        await _repository.UpdateAsync(document);
        _logger.LogInformation("Document deactivated: Id {DocumentId}", id);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        _logger.LogWarning("Document deleted: Id {DocumentId}", id);
    }

    public async Task<IEnumerable<Document>> GetExpiredAsync()
    {
        var documents = await _repository.GetAllAsync();
        var today = DateTime.Today;
        return documents
            .Where(d => d.IsActive && d.ExpirationDate.Date < today)
            .OrderBy(d => d.ExpirationDate);
    }

    public async Task<IEnumerable<Document>> GetExpiringSoonAsync(int daysAhead)
    {
        var documents = await _repository.GetAllAsync();
        var today = DateTime.Today;
        var cutoff = today.AddDays(daysAhead);
        return documents
            .Where(d => d.IsActive && d.ExpirationDate.Date >= today && d.ExpirationDate.Date <= cutoff)
            .OrderBy(d => d.ExpirationDate);
    }

    public async Task<IEnumerable<Document>> GetDueForReminderAsync()
    {
        var documents = await _repository.GetAllAsync();
        var today = DateTime.Today;
        return documents
            .Where(d => d.IsActive && d.ReminderDate.HasValue && d.ReminderDate.Value.Date <= today)
            .OrderBy(d => d.ReminderDate);
    }
}
