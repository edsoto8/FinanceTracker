using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Data.Repositories;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests;

public class DocumentServiceTests
{
    [Fact]
    public async Task GetExpiringSoon_ReturnsDocumentsWithinWindow()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new DocumentService(new DocumentRepository(cs));

        await service.AddDocumentAsync(new Document { Name = "Insurance", Type = DocumentType.Insurance, ExpirationDate = DateTime.UtcNow.AddDays(15), IsActive = true });
        await service.AddDocumentAsync(new Document { Name = "License", Type = DocumentType.License, ExpirationDate = DateTime.UtcNow.AddDays(60), IsActive = true });

        var expiring = (await service.GetExpiringSoonAsync(30)).ToList();

        Assert.Single(expiring);
        Assert.Equal("Insurance", expiring[0].Name);
    }

    [Fact]
    public async Task GetExpired_ReturnsOnlyExpiredDocuments()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new DocumentService(new DocumentRepository(cs));

        await service.AddDocumentAsync(new Document { Name = "Old", Type = DocumentType.License, ExpirationDate = DateTime.UtcNow.AddDays(-10), IsActive = true });
        await service.AddDocumentAsync(new Document { Name = "Current", Type = DocumentType.Insurance, ExpirationDate = DateTime.UtcNow.AddDays(30), IsActive = true });

        var expired = (await service.GetExpiredAsync()).ToList();

        Assert.Single(expired);
        Assert.Equal("Old", expired[0].Name);
    }

    [Fact]
    public async Task AddDocument_PersistsAndReturnsWithId()
    {
        var cs = await TestDatabase.CreateAsync();
        var service = new DocumentService(new DocumentRepository(cs));

        var doc = await service.AddDocumentAsync(new Document { Name = "Warranty", Type = DocumentType.Warranty, ExpirationDate = DateTime.Today.AddYears(1), IsActive = true });

        Assert.True(doc.Id > 0);
    }
}
