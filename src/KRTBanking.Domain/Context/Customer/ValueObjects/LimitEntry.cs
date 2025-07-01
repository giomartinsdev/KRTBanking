using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.ValueObjects;

/// <summary>
/// Represents a single limit adjustment entry as a value object.
/// </summary>
public sealed class LimitEntry : IValueObject, IEquatable<LimitEntry>
{
    /// <summary>
    /// Gets the limit adjustment amount (can be positive or negative).
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the description of the limit adjustment.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the timestamp when the limit adjustment was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LimitEntry"/> class.
    /// </summary>
    /// <param name="amount">The limit adjustment amount.</param>
    /// <param name="description">The description of the adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when description is null or whitespace.</exception>
    public LimitEntry(decimal amount, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Limit entry description cannot be null or empty.", nameof(description));
        }

        Amount = amount;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new limit entry with the specified amount and description.
    /// </summary>
    /// <param name="amount">The limit adjustment amount.</param>
    /// <param name="description">The description of the adjustment.</param>
    /// <returns>A new <see cref="LimitEntry"/> instance.</returns>
    public static LimitEntry Create(decimal amount, string description)
    {
        return new LimitEntry(amount, description);
    }

    /// <summary>
    /// Creates a new positive limit adjustment entry.
    /// </summary>
    /// <param name="amount">The positive amount to add.</param>
    /// <param name="description">The description of the adjustment.</param>
    /// <returns>A new <see cref="LimitEntry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is not positive.</exception>
    public static LimitEntry CreateIncrease(decimal amount, string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Increase amount must be positive.");
        }

        return new LimitEntry(amount, description);
    }

    /// <summary>
    /// Creates a new negative limit adjustment entry.
    /// </summary>
    /// <param name="amount">The positive amount to subtract (will be made negative).</param>
    /// <param name="description">The description of the adjustment.</param>
    /// <returns>A new <see cref="LimitEntry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is not positive.</exception>
    public static LimitEntry CreateDecrease(decimal amount, string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Decrease amount must be positive.");
        }

        return new LimitEntry(-amount, description);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current limit entry.
    /// </summary>
    /// <param name="obj">The object to compare with the current limit entry.</param>
    /// <returns>true if the specified object is equal to the current limit entry; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as LimitEntry);
    }

    /// <summary>
    /// Determines whether the specified limit entry is equal to the current limit entry.
    /// </summary>
    /// <param name="other">The limit entry to compare with the current limit entry.</param>
    /// <returns>true if the specified limit entry is equal to the current limit entry; otherwise, false.</returns>
    public bool Equals(LimitEntry? other)
    {
        return other is not null &&
               Amount == other.Amount &&
               Description == other.Description &&
               CreatedAt == other.CreatedAt;
    }

    /// <summary>
    /// Returns a hash code for the current limit entry.
    /// </summary>
    /// <returns>A hash code for the current limit entry.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Description, CreatedAt);
    }

    /// <summary>
    /// Returns a string representation of the limit entry.
    /// </summary>
    /// <returns>A string representation of the limit entry.</returns>
    public override string ToString()
    {
        var adjustment = Amount >= 0 ? "+" : string.Empty;
        return $"Limit Entry: {adjustment}{Amount:C} - {Description} ({CreatedAt:yyyy-MM-dd HH:mm:ss})";
    }

    /// <summary>
    /// Determines whether two limit entries are equal.
    /// </summary>
    /// <param name="left">The first limit entry to compare.</param>
    /// <param name="right">The second limit entry to compare.</param>
    /// <returns>true if the limit entries are equal; otherwise, false.</returns>
    public static bool operator ==(LimitEntry? left, LimitEntry? right)
    {
        return EqualityComparer<LimitEntry>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two limit entries are not equal.
    /// </summary>
    /// <param name="left">The first limit entry to compare.</param>
    /// <param name="right">The second limit entry to compare.</param>
    /// <returns>true if the limit entries are not equal; otherwise, false.</returns>
    public static bool operator !=(LimitEntry? left, LimitEntry? right)
    {
        return !(left == right);
    }
}
