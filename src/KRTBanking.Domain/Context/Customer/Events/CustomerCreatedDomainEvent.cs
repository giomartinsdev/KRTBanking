using BankingProject.Domain.Abstractions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Events;

/// <summary>
/// Domain event raised when a new customer is created.
/// </summary>
public sealed class CustomerCreatedDomainEvent : IDomainEvent
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
    public string EventType => nameof(CustomerCreatedDomainEvent);

    /// <summary>
    /// Gets the identifier of the created customer.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// Gets the document number of the created customer.
    /// </summary>
    public DocumentNumber DocumentNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerCreatedDomainEvent"/> class.
    /// </summary>
    /// <param name="customerId">The identifier of the created customer.</param>
    /// <param name="documentNumber">The document number of the created customer.</param>
    public CustomerCreatedDomainEvent(Guid customerId, DocumentNumber documentNumber)
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        CustomerId = customerId;
        DocumentNumber = documentNumber;
    }
}
