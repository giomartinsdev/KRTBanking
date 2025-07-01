using BankingProject.Domain.Abstractions;

namespace KRTBanking.Domain.Context.Customer.ValueObjects;

/// <summary>
/// Represents a bank account as a value object.
/// </summary>
public sealed class Account : IValueObject, IEquatable<Account>
{
    /// <summary>
    /// Gets the agency of the account.
    /// </summary>
    public Agency Agency { get; }

    /// <summary>
    /// Gets the account number.
    /// </summary>
    public int AccountNumber { get; }

    /// <summary>
    /// Gets the account number as a string for API compatibility.
    /// </summary>
    public string Number => FormattedAccount;

    /// <summary>
    /// Gets the timestamp when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the formatted account string representation.
    /// </summary>
    public string FormattedAccount => $"{(int)Agency:D4}-{AccountNumber:D8}";

    /// <summary>
    /// Initializes a new instance of the <see cref="Account"/> class.
    /// </summary>
    /// <param name="agency">The agency.</param>
    /// <param name="accountNumber">The account number.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when account number is invalid.</exception>
    public Account(Agency agency, int accountNumber)
    {
        if (!Enum.IsDefined(typeof(Agency), agency))
        {
            throw new ArgumentOutOfRangeException(nameof(agency), agency, "Invalid agency value.");
        }

        if (accountNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accountNumber), accountNumber, "Account number must be positive.");
        }

        Agency = agency;
        AccountNumber = accountNumber;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new account with the specified agency and account number.
    /// </summary>
    /// <param name="agency">The agency.</param>
    /// <param name="accountNumber">The account number.</param>
    /// <returns>A new <see cref="Account"/> instance.</returns>
    public static Account Create(Agency agency, int accountNumber)
    {
        return new Account(agency, accountNumber);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current account.
    /// </summary>
    /// <param name="obj">The object to compare with the current account.</param>
    /// <returns>true if the specified object is equal to the current account; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Account);
    }

    /// <summary>
    /// Determines whether the specified account is equal to the current account.
    /// </summary>
    /// <param name="other">The account to compare with the current account.</param>
    /// <returns>true if the specified account is equal to the current account; otherwise, false.</returns>
    public bool Equals(Account? other)
    {
        return other is not null &&
               Agency == other.Agency &&
               AccountNumber == other.AccountNumber;
    }

    /// <summary>
    /// Returns a hash code for the current account.
    /// </summary>
    /// <returns>A hash code for the current account.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Agency, AccountNumber);
    }

    /// <summary>
    /// Returns a string representation of the account.
    /// </summary>
    /// <returns>A string representation of the account.</returns>
    public override string ToString()
    {
        return FormattedAccount;
    }

    /// <summary>
    /// Determines whether two accounts are equal.
    /// </summary>
    /// <param name="left">The first account to compare.</param>
    /// <param name="right">The second account to compare.</param>
    /// <returns>true if the accounts are equal; otherwise, false.</returns>
    public static bool operator ==(Account? left, Account? right)
    {
        return EqualityComparer<Account>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two accounts are not equal.
    /// </summary>
    /// <param name="left">The first account to compare.</param>
    /// <param name="right">The second account to compare.</param>
    /// <returns>true if the accounts are not equal; otherwise, false.</returns>
    public static bool operator !=(Account? left, Account? right)
    {
        return !(left == right);
    }
}