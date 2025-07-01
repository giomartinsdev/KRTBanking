namespace BankingProject.Domain.Abstractions;

/// <summary>
/// Represents an aggregate root in Domain-Driven Design.
/// An aggregate root is the only entity that external objects are allowed to hold references to.
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// Gets the domain events that have been raised by this aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// </summary>
    void ClearDomainEvents();

    /// <summary>
    /// Gets the creation timestamp of the aggregate.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the last update timestamp of the aggregate.
    /// </summary>
    DateTime UpdatedAt { get; }

    /// <summary>
    /// Gets the version for optimistic concurrency control.
    /// </summary>
    long Version { get; }
}
