using System.ComponentModel.DataAnnotations;

namespace KRTBanking.Application.DTOs.Transaction;

/// <summary>
/// Data transfer object for executing a transaction.
/// </summary>
public sealed class ExecuteTransactionDto
{
    /// <summary>
    /// Gets or sets the merchant document number.
    /// </summary>
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public required string MerchantDocument { get; set; }

    /// <summary>
    /// Gets or sets the transaction value.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transaction value must be greater than zero")]
    public decimal Value { get; set; }
}
