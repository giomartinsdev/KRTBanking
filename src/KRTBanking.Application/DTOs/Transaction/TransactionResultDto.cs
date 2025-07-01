namespace KRTBanking.Application.DTOs.Transaction;

/// <summary>
/// Data transfer object for transaction execution result.
/// </summary>
public sealed class TransactionResultDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the transaction was authorized.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Gets or sets the reason for authorization or denial.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's remaining limit after the transaction.
    /// </summary>
    public decimal? RemainingLimit { get; set; }

    /// <summary>
    /// Gets or sets the transaction value.
    /// </summary>
    public decimal TransactionValue { get; set; }
}
