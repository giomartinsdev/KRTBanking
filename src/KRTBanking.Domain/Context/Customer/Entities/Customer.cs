using BankingProject.Domain.Abstractions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Entities;

public class Customer : IAggregateRoot
{
    public required Guid Id { get; set; }
    public required DocumentNumber DocumentNumber { get; set; }
    public Account? Account { get; set; }
    public required IEnumerable<Limit> Limit { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    #region Calculated Properties
    public int LimitAmount => Limit?.Sum(limit => limit.Amount) ?? 0;
    #endregion

    public Customer(Guid id, string documentNumber, Account? account)
    {
        Id = id;
        DocumentNumber = DocumentNumber.TryCreate(documentNumber) ?? throw new ArgumentException("Invalid document number format.", nameof(documentNumber));
        Account = account;
    }
}