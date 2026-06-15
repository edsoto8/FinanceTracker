using FinanceTracker.Core.Models;
using FinanceTracker.Core.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinanceTracker.Tests;

public class DocumentServiceTests
{
    private static DocumentService CreateService(TestDatabase db)
        => new(db.Documents, NullLogger<DocumentService>.Instance);

    [Fact]
    public async Task GetExpired_ReturnsOnlyPastActiveDocuments()
    {
        using var db = new TestDatabase();
        var service = CreateService(db);

        await service.AddAsync(new Document
        {
            Name = "Old",
            Type = DocumentType.License,
            ExpirationDate = DateTime.Today.AddDays(-5)
        });
        await service.AddAsync(new Document
        {
            Name = "Valid",
            Type = DocumentType.Insurance,
            ExpirationDate = DateTime.Today.AddDays(30)
        });

        var expired = (await service.GetExpiredAsync()).ToList();

        Assert.Single(expired);
        Assert.Equal("Old", expired[0].Name);
    }

    [Fact]
    public async Task GetExpiringSoon_RespectsWindow()
    {
        using var db = new TestDatabase();
        var service = CreateService(db);

        await service.AddAsync(new Document
        {
            Name = "Soon",
            Type = DocumentType.Registration,
            ExpirationDate = DateTime.Today.AddDays(10)
        });
        await service.AddAsync(new Document
        {
            Name = "Far",
            Type = DocumentType.Warranty,
            ExpirationDate = DateTime.Today.AddDays(90)
        });

        var soon = (await service.GetExpiringSoonAsync(30)).ToList();

        Assert.Single(soon);
        Assert.Equal("Soon", soon[0].Name);
    }

    [Fact]
    public async Task GetDueForReminder_ReturnsRemindersOnOrBeforeToday()
    {
        using var db = new TestDatabase();
        var service = CreateService(db);

        await service.AddAsync(new Document
        {
            Name = "DueToday",
            Type = DocumentType.Contract,
            ExpirationDate = DateTime.Today.AddDays(15),
            ReminderDate = DateTime.Today
        });
        await service.AddAsync(new Document
        {
            Name = "FutureReminder",
            Type = DocumentType.Contract,
            ExpirationDate = DateTime.Today.AddDays(40),
            ReminderDate = DateTime.Today.AddDays(20)
        });

        var due = (await service.GetDueForReminderAsync()).ToList();

        Assert.Single(due);
        Assert.Equal("DueToday", due[0].Name);
    }
}
