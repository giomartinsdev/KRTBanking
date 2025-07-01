namespace KRTBanking.Infrastructure.Data.Models;

/// <summary>
/// Limit entry model for JSON serialization.
/// </summary>
public sealed class LimitEntryModel
{
    /// <summary>
    /// Gets or sets the limit adjustment amount (can be positive or negative).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the description of the adjustment.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
