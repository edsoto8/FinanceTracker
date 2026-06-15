namespace FinanceTracker.Core.Models;

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public BillingFrequency Frequency { get; set; }
    public DateTime NextBillingDate { get; set; }
    public int CategoryId { get; set; }
    public int? AccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public enum BillingFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
