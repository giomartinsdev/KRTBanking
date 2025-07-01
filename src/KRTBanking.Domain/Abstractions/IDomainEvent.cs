namespace BankingProject.Domain.Abstractions;

/// <summary>
/// Represents a domain event that occurred within the domain.
/// Domain events are used to capture and communicate important business occurrences.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the timestamp when the domain event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the name of the event type.
    /// </summary>
    string EventType { get; }
}
