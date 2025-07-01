using BankingProject.Domain.Abstractions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Events;

/// <summary>
/// Domain event raised when a customer's limit is updated.
/// </summary>
public sealed class CustomerLimitUpdatedDomainEvent : IDomainEvent
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
    public string EventType => nameof(CustomerLimitUpdatedDomainEvent);

    /// <summary>
    /// Gets the identifier of the customer.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// Gets the new limit entries that were added.
    /// </summary>
    public IReadOnlyList<LimitEntry> NewLimitEntries { get; }

    /// <summary>
    /// Gets the current total limit amount.
    /// </summary>
    public decimal CurrentTotalLimit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerLimitUpdatedDomainEvent"/> class.
    /// </summary>
    /// <param name="customerId">The identifier of the customer.</param>
    /// <param name="newLimitEntries">The new limit entries that were added.</param>
    /// <param name="currentTotalLimit">The current total limit amount.</param>
    public CustomerLimitUpdatedDomainEvent(Guid customerId, IReadOnlyList<LimitEntry> newLimitEntries, decimal currentTotalLimit)
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        CustomerId = customerId;
        NewLimitEntries = newLimitEntries;
        CurrentTotalLimit = currentTotalLimit;
    }
}
