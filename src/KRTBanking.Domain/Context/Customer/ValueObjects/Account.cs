using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.ValueObjects;

public class Account : IValueObject
{
    public required int AgencyCode { get; set; }
    public required int AccountNumber { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    public Account(int agencyCode, int accountNumber)
    {
        AgencyCode = agencyCode;
        AccountNumber = accountNumber;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}