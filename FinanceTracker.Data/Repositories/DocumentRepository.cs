using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;
using Microsoft.Data.Sqlite;

namespace FinanceTracker.Data.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly string _connectionString;

    public DocumentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SqliteConnection Open() => new(_connectionString);

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Document>("SELECT * FROM Documents ORDER BY ExpirationDate");
    }

    public async Task<IEnumerable<Document>> GetActiveAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Document>(
            "SELECT * FROM Documents WHERE IsActive = 1 ORDER BY ExpirationDate");
    }

    public async Task<IEnumerable<Document>> GetExpiredAsync()
    {
        using var conn = Open();
        return await conn.QueryAsync<Document>(
            "SELECT * FROM Documents WHERE IsActive = 1 AND ExpirationDate < @Now ORDER BY ExpirationDate",
            new { Now = DateTime.UtcNow });
    }

    public async Task<IEnumerable<Document>> GetExpiringSoonAsync(int days)
    {
        using var conn = Open();
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);
        return await conn.QueryAsync<Document>(
            "SELECT * FROM Documents WHERE IsActive = 1 AND ExpirationDate >= @Now AND ExpirationDate <= @Cutoff ORDER BY ExpirationDate",
            new { Now = now, Cutoff = cutoff });
    }

    public async Task<IEnumerable<Document>> GetWithUpcomingRemindersAsync(int days)
    {
        using var conn = Open();
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);
        return await conn.QueryAsync<Document>(
            "SELECT * FROM Documents WHERE IsActive = 1 AND ReminderDate >= @Now AND ReminderDate <= @Cutoff ORDER BY ReminderDate",
            new { Now = now, Cutoff = cutoff });
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        using var conn = Open();
        return await conn.QuerySingleOrDefaultAsync<Document>(
            "SELECT * FROM Documents WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> AddAsync(Document document)
    {
        using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Documents (Name, Type, ExpirationDate, ReminderDate, Notes, IsActive)
            VALUES (@Name, @Type, @ExpirationDate, @ReminderDate, @Notes, @IsActive);
            SELECT last_insert_rowid();", document);
    }

    public async Task UpdateAsync(Document document)
    {
        using var conn = Open();
        await conn.ExecuteAsync(@"
            UPDATE Documents SET Name=@Name, Type=@Type, ExpirationDate=@ExpirationDate,
            ReminderDate=@ReminderDate, Notes=@Notes, IsActive=@IsActive WHERE Id=@Id", document);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = Open();
        await conn.ExecuteAsync("DELETE FROM Documents WHERE Id = @Id", new { Id = id });
    }
}
