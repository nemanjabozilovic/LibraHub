namespace LibraHub.Orders.Infrastructure.Options;

public class MockPaymentOptions
{
    public const string SectionName = "MockPayment";

    /// <summary>
    /// Probability of payment failure (0-100). 0 means always succeed, 100 means always fail.
    /// </summary>
    public int FailureProbabilityPercent { get; set; } = 0;

    /// <summary>
    /// If true, payments with amount ending in specific digits will fail (e.g., .99, .98)
    /// </summary>
    public bool UseAmountBasedFailure { get; set; } = false;

    /// <summary>
    /// Amount endings that trigger failure (e.g., ["99", "98"] means amounts ending in .99 or .98 will fail)
    /// </summary>
    public List<string> FailureAmountEndings { get; set; } = new() { "99", "98" };

    /// <summary>
    /// Failure reasons to randomly select from
    /// </summary>
    public List<string> FailureReasons { get; set; } = new()
    {
        "Insufficient funds",
        "Card declined",
        "Payment gateway timeout",
        "Invalid card details",
        "Transaction limit exceeded",
        "Network error"
    };
}

