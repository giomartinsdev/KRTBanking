namespace BankingProject.Domain.Abstractions;

/// <summary>
/// Represents a domain entity with unique identity.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Id { get; }
}
