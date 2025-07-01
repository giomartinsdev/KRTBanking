using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.ValueObjects;

/// <summary>
/// Represents a customer's credit limit as a value object.
/// </summary>
public sealed class Limit : IValueObject, IEquatable<Limit>
{
    /// <summary>
    /// Gets the limit amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the description of the limit.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the timestamp when the limit was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Limit"/> class.
    /// </summary>
    /// <param name="amount">The limit amount.</param>
    /// <param name="description">The description of the limit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    /// <exception cref="ArgumentException">Thrown when description is null or whitespace.</exception>
    public Limit(decimal amount, string description)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Limit amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Limit description cannot be null or empty.", nameof(description));
        }

        Amount = amount;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Limit"/> class with a default description.
    /// </summary>
    /// <param name="amount">The limit amount.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public Limit(decimal amount) : this(amount, "Credit Limit")
    {
    }

    /// <summary>
    /// Creates a new limit with the specified amount and description.
    /// </summary>
    /// <param name="amount">The limit amount.</param>
    /// <param name="description">The description of the limit.</param>
    /// <returns>A new <see cref="Limit"/> instance.</returns>
    public static Limit Create(decimal amount, string description)
    {
        return new Limit(amount, description);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current limit.
    /// </summary>
    /// <param name="obj">The object to compare with the current limit.</param>
    /// <returns>true if the specified object is equal to the current limit; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Limit);
    }

    /// <summary>
    /// Determines whether the specified limit is equal to the current limit.
    /// </summary>
    /// <param name="other">The limit to compare with the current limit.</param>
    /// <returns>true if the specified limit is equal to the current limit; otherwise, false.</returns>
    public bool Equals(Limit? other)
    {
        return other is not null &&
               Amount == other.Amount &&
               Description == other.Description;
    }

    /// <summary>
    /// Returns a hash code for the current limit.
    /// </summary>
    /// <returns>A hash code for the current limit.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Description);
    }

    /// <summary>
    /// Returns a string representation of the limit.
    /// </summary>
    /// <returns>A string representation of the limit.</returns>
    public override string ToString()
    {
        return $"Limit: {Amount:C} - {Description}";
    }

    /// <summary>
    /// Determines whether two limits are equal.
    /// </summary>
    /// <param name="left">The first limit to compare.</param>
    /// <param name="right">The second limit to compare.</param>
    /// <returns>true if the limits are equal; otherwise, false.</returns>
    public static bool operator ==(Limit? left, Limit? right)
    {
        return EqualityComparer<Limit>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two limits are not equal.
    /// </summary>
    /// <param name="left">The first limit to compare.</param>
    /// <param name="right">The second limit to compare.</param>
    /// <returns>true if the limits are not equal; otherwise, false.</returns>
    public static bool operator !=(Limit? left, Limit? right)
    {
        return !(left == right);
    }
}