namespace FinanceTracker.Core.Models;

public enum BillingFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public BillingFrequency Frequency { get; set; }
    public DateTime NextBillingDate { get; set; }
    public string? Category { get; set; }
    public int? PaymentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public decimal MonthlyCost => Frequency switch
    {
        BillingFrequency.Weekly    => Cost * 52 / 12,
        BillingFrequency.Monthly   => Cost,
        BillingFrequency.Quarterly => Cost / 3,
        BillingFrequency.Yearly    => Cost / 12,
        _                          => Cost
    };
}
