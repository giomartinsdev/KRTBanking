using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.ValueObjects;

public class Limit : IValueObject
{
    public required int Amount { get; set; }
    public required string Description { get; set; }
    public required DateTime CreatedAt { get; set; }

    public Limit(int amount, string? description)
    {
        Amount = amount;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }
}