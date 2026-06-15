namespace FinanceTracker.Core.Models;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum DocumentType
{
    Insurance,
    License,
    Registration,
    Warranty,
    Contract,
    Other
}
