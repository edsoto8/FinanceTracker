namespace FinanceTracker.Core.Models;

public class BalanceEntry
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime BalanceDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
