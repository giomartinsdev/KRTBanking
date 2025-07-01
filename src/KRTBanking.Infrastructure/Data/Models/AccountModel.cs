namespace KRTBanking.Infrastructure.Data.Models;


/// <summary>
/// Account model for JSON serialization.
/// </summary>
public sealed class AccountModel
{
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}