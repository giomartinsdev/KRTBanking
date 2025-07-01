using BankingProject.Domain.Abstractions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Events;

/// <summary>
/// Domain event raised when a customer's account information is updated.
/// </summary>
public sealed class CustomerAccountUpdatedDomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the timestamp when the domain event occurred.
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the name of the event type.
    /// </summary>
    public string EventType => nameof(CustomerAccountUpdatedDomainEvent);

    /// <summary>
    /// Gets the identifier of the customer.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// Gets the new account information.
    /// </summary>
    public Account? Account { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerAccountUpdatedDomainEvent"/> class.
    /// </summary>
    /// <param name="customerId">The identifier of the customer.</param>
    /// <param name="account">The new account information.</param>
    public CustomerAccountUpdatedDomainEvent(Guid customerId, Account? account)
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        CustomerId = customerId;
        Account = account;
    }
}
