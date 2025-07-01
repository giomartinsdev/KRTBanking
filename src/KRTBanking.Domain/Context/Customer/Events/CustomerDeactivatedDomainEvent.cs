using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.Events;

/// <summary>
/// Domain event raised when a customer is deactivated (soft deleted).
/// This event captures the business significance of customer deactivation for audit and compliance.
/// </summary>
public sealed class CustomerDeactivatedDomainEvent : IDomainEvent
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
    public string EventType => nameof(CustomerDeactivatedDomainEvent);

    /// <summary>
    /// Gets the identifier of the deactivated customer.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// Gets the reason for customer deactivation.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the identifier of the user/system that initiated the deactivation.
    /// </summary>
    public string? DeactivatedBy { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerDeactivatedDomainEvent"/> class.
    /// </summary>
    /// <param name="customerId">The identifier of the deactivated customer.</param>
    /// <param name="reason">The reason for customer deactivation.</param>
    /// <param name="deactivatedBy">The identifier of the user/system that initiated the deactivation.</param>
    public CustomerDeactivatedDomainEvent(Guid customerId, string reason, string? deactivatedBy = null)
    {
        Id = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        CustomerId = customerId;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        DeactivatedBy = deactivatedBy;
    }
}
