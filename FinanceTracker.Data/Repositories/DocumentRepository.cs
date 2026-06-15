using Dapper;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Data.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly SqliteConnectionFactory _factory;

    public DocumentRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        using var connection = _factory.Create();
        return await connection.QueryAsync<Document>(
            "SELECT * FROM Documents ORDER BY ExpirationDate;");
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        using var connection = _factory.Create();
        return await connection.QuerySingleOrDefaultAsync<Document>(
            "SELECT * FROM Documents WHERE Id = @id;", new { id });
    }

    public async Task<int> InsertAsync(Document document)
    {
        using var connection = _factory.Create();
        return await connection.ExecuteScalarAsync<int>(
            """
            INSERT INTO Documents (Name, Type, ExpirationDate, ReminderDate, Notes, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Type, @ExpirationDate, @ReminderDate, @Notes, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();
            """, document);
    }

    public async Task UpdateAsync(Document document)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync(
            """
            UPDATE Documents
            SET Name = @Name, Type = @Type, ExpirationDate = @ExpirationDate, ReminderDate = @ReminderDate,
                Notes = @Notes, IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """, document);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.Create();
        await connection.ExecuteAsync("DELETE FROM Documents WHERE Id = @id;", new { id });
    }
}
