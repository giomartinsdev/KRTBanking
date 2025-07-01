using BankingProject.Domain.Abstractions;
using KRTBanking.Domain.Context.Customer.Events;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Entities;

/// <summary>
/// Represents a customer aggregate root in the banking domain.
/// This aggregate encapsulates customer-related business logic and maintains consistency boundaries.
/// </summary>
public sealed class Customer : AggregateRoot
{
    /// <summary>
    /// Gets the customer's document number.
    /// </summary>
    public DocumentNumber DocumentNumber { get; private set; }

    /// <summary>
    /// Gets the customer's name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the customer's email address.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the customer's account information.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Gets the customer's limit entries collection (append-only).
    /// </summary>
    public IReadOnlyList<LimitEntry> LimitEntries { get; private set; }

    /// <summary>
    /// Gets the customer's current total limit calculated from all limit entries.
    /// </summary>
    public decimal CurrentLimit => LimitEntries.Sum(entry => entry.Amount);

    /// <summary>
    /// Gets a value indicating whether the customer is active.
    /// Inactive customers cannot perform banking operations.
    /// </summary>
    public bool IsActive { get; private set; }

    private readonly List<LimitEntry> _limitEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class for existing customers (from repository).
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="documentNumber">The customer's document number.</param>
    /// <param name="name">The customer's name.</param>
    /// <param name="email">The customer's email address.</param>
    /// <param name="account">The customer's account information.</param>
    /// <param name="limitEntries">The customer's limit entries collection.</param>
    /// <param name="isActive">Indicates whether the customer is active.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="updatedAt">The last update timestamp.</param>
    /// <param name="version">The version for optimistic concurrency control.</param>
    private Customer(
        Guid id,
        DocumentNumber documentNumber,
        string name,
        string email,
        Account account,
        IEnumerable<LimitEntry> limitEntries,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        long version)
    {
        Id = id;
        DocumentNumber = documentNumber;
        Name = name;
        Email = email;
        Account = account;
        _limitEntries = limitEntries.ToList();
        LimitEntries = _limitEntries.AsReadOnly();
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class for new customers.
    /// </summary>
    /// <param name="documentNumber">The customer's document number.</param>
    /// <param name="name">The customer's name.</param>
    /// <param name="email">The customer's email address.</param>
    /// <param name="account">The customer's account information.</param>
    /// <param name="initialLimitAmount">The initial limit amount.</param>
    /// <param name="initialLimitDescription">The initial limit description.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name or email is empty or whitespace.</exception>
    private Customer(
        DocumentNumber documentNumber,
        string name,
        string email,
        Account account,
        decimal initialLimitAmount,
        string initialLimitDescription)
    {
        ArgumentNullException.ThrowIfNull(documentNumber);
        ArgumentNullException.ThrowIfNull(account);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name cannot be null or empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Customer email cannot be null or empty.", nameof(email));
        }

        DocumentNumber = documentNumber;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Account = account;
        IsActive = true;
        
        _limitEntries = new List<LimitEntry>();
        if (initialLimitAmount > 0)
        {
            _limitEntries.Add(new LimitEntry(initialLimitAmount, initialLimitDescription));
        }
        LimitEntries = _limitEntries.AsReadOnly();
        
        // Raise domain event for customer creation
        AddDomainEvent(new CustomerCreatedDomainEvent(Id, DocumentNumber));
    }

    /// <summary>
    /// Creates a new customer with the specified information.
    /// </summary>
    /// <param name="documentNumber">The customer's document number.</param>
    /// <param name="name">The customer's name.</param>
    /// <param name="email">The customer's email address.</param>
    /// <param name="account">The customer's account information.</param>
    /// <param name="initialLimitAmount">The initial limit amount.</param>
    /// <param name="initialLimitDescription">The initial limit description.</param>
    /// <returns>A new <see cref="Customer"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name or email is empty or whitespace.</exception>
    public static Customer Create(
        DocumentNumber documentNumber,
        string name,
        string email,
        Account account,
        decimal initialLimitAmount,
        string initialLimitDescription)
    {
        return new Customer(documentNumber, name, email, account, initialLimitAmount, initialLimitDescription);
    }

    /// <summary>
    /// Reconstructs a customer from repository data.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="documentNumber">The customer's document number.</param>
    /// <param name="name">The customer's name.</param>
    /// <param name="email">The customer's email address.</param>
    /// <param name="account">The customer's account information.</param>
    /// <param name="limitEntries">The customer's limit entries collection.</param>
    /// <param name="isActive">Indicates whether the customer is active.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="updatedAt">The last update timestamp.</param>
    /// <param name="version">The version for optimistic concurrency control.</param>
    /// <returns>A reconstructed <see cref="Customer"/> instance.</returns>
    public static Customer Reconstruct(
        Guid id,
        DocumentNumber documentNumber,
        string name,
        string email,
        Account account,
        IEnumerable<LimitEntry> limitEntries,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        long version)
    {
        ArgumentNullException.ThrowIfNull(documentNumber);

        return new Customer(id, documentNumber, name, email, account, limitEntries, isActive, createdAt, updatedAt, version);
    }

    /// <summary>
    /// Updates the customer's account information.
    /// </summary>
    /// <param name="account">The new account information.</param>
    /// <exception cref="ArgumentNullException">Thrown when account is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when customer is inactive.</exception>
    public void UpdateAccount(Account account)
    {
        EnsureCustomerIsActive();
        ArgumentNullException.ThrowIfNull(account);

        if (Account.Equals(account))
        {
            return;
        }

        Account = account;
        MarkAsModified();

        AddDomainEvent(new CustomerAccountUpdatedDomainEvent(Id, Account));
    }

    /// <summary>
    /// Adds a new limit adjustment entry.
    /// </summary>
    /// <param name="amount">The adjustment amount (positive for increase, negative for decrease).</param>
    /// <param name="description">The description of the adjustment.</param>
    /// <exception cref="ArgumentException">Thrown when description is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when customer is inactive.</exception>
    public void AdjustLimit(decimal amount, string description)
    {
        EnsureCustomerIsActive();
        
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Limit adjustment description cannot be null or empty.", nameof(description));
        }

        var limitEntry = new LimitEntry(amount, description);
        _limitEntries.Add(limitEntry);
        MarkAsModified();

        // Raise domain event for limit update
        AddDomainEvent(new CustomerLimitUpdatedDomainEvent(Id, new[] { limitEntry }, CurrentLimit));
    }

    /// <summary>
    /// Increases the customer's limit by adding a positive adjustment entry.
    /// </summary>
    /// <param name="amount">The amount to increase (must be positive).</param>
    /// <param name="description">The description of the increase.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when description is null or whitespace.</exception>
    public void IncreaseLimit(decimal amount, string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Increase amount must be positive.");
        }

        AdjustLimit(amount, description);
    }

    /// <summary>
    /// Decreases the customer's limit by adding a negative adjustment entry.
    /// </summary>
    /// <param name="amount">The amount to decrease (must be positive).</param>
    /// <param name="description">The description of the decrease.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when description is null or whitespace.</exception>
    public void DecreaseLimit(decimal amount, string description)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Decrease amount must be positive.");
        }

        AdjustLimit(-amount, description);
    }

    /// <summary>
    /// Deactivates the customer account (soft delete).
    /// Once deactivated, the customer cannot perform banking operations but data is retained for compliance.
    /// </summary>
    /// <param name="reason">The reason for deactivation.</param>
    /// <param name="deactivatedBy">The identifier of the user/system initiating the deactivation.</param>
    /// <exception cref="ArgumentException">Thrown when reason is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when customer is already inactive.</exception>
    public void Deactivate(string reason, string? deactivatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Deactivation reason cannot be null or empty.", nameof(reason));
        }

        if (!IsActive)
        {
            throw new InvalidOperationException("Customer is already inactive.");
        }

        IsActive = false;
        MarkAsModified();

        AddDomainEvent(new CustomerDeactivatedDomainEvent(Id, reason, deactivatedBy));
    }

    /// <summary>
    /// Reactivates a previously deactivated customer.
    /// </summary>
    /// <param name="reason">The reason for reactivation.</param>
    /// <param name="reactivatedBy">The identifier of the user/system initiating the reactivation.</param>
    /// <exception cref="ArgumentException">Thrown when reason is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when customer is already active.</exception>
    public void Reactivate(string reason, string? reactivatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reactivation reason cannot be null or empty.", nameof(reason));
        }

        if (IsActive)
        {
            throw new InvalidOperationException("Customer is already active.");
        }

        IsActive = true;
        MarkAsModified();

    }

    /// <summary>
    /// Ensures that business operations can only be performed on active customers.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when customer is inactive.</exception>
    private void EnsureCustomerIsActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot perform operations on an inactive customer.");
        }
    }
}