namespace BankingProject.Domain.Abstractions;

/// <summary>
/// Base implementation for aggregate roots providing common functionality.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the unique identifier of the aggregate.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets the creation timestamp of the aggregate.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the last update timestamp of the aggregate.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets the version for optimistic concurrency control.
    /// </summary>
    public long Version { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// </summary>
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate.</param>
    protected AggregateRoot(Guid id)
    {
        Id = id;
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;
    }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Adds a domain event to this aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Updates the aggregate's timestamp and increments the version.
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }
}
