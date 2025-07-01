using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Application.DTOs.Customer;

/// <summary>
/// Data transfer object for customer information.
/// </summary>
public sealed class CustomerDto
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's document number.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account information.
    /// </summary>
    public AccountDto Account { get; set; } = new();

    /// <summary>
    /// Gets or sets the limit entries collection.
    /// </summary>
    public IReadOnlyList<LimitEntryDto> LimitEntries { get; set; } = new List<LimitEntryDto>();

    /// <summary>
    /// Gets the current total limit calculated from all limit entries.
    /// </summary>
    public decimal CurrentLimit => LimitEntries.Sum(entry => entry.Amount);

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the version for optimistic concurrency control.
    /// </summary>
    public long Version { get; set; }
}

/// <summary>
/// Data transfer object for creating a new customer.
/// </summary>
public sealed class CreateCustomerDto
{
    /// <summary>
    /// Gets or sets the customer's document number.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agency.
    /// </summary>
    public Agency Agency { get; set; } = Agency.Agency1;

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public int AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the limit amount.
    /// </summary>
    public decimal LimitAmount { get; set; }

    /// <summary>
    /// Gets or sets the limit description.
    /// </summary>
    public string LimitDescription { get; set; } = "Credit Limit";
}

/// <summary>
/// Data transfer object for account information.
/// </summary>
public sealed class AccountDto
{
    /// <summary>
    /// Gets or sets the agency.
    /// </summary>
    public Agency Agency { get; set; } = Agency.Agency1;

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public int AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the formatted account number.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for limit entry information.
/// </summary>
public sealed class LimitEntryDto
{
    /// <summary>
    /// Gets or sets the limit adjustment amount (can be positive or negative).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the description of the limit adjustment.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for limit information.
/// </summary>
public sealed class LimitDto
{
    /// <summary>
    /// Gets or sets the limit amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the limit description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for updating account information.
/// </summary>
public sealed class UpdateAccountDto
{
    /// <summary>
    /// Gets or sets the agency.
    /// </summary>
    public Agency Agency { get; set; } = Agency.Agency1;

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public int AccountNumber { get; set; }
}

/// <summary>
/// Data transfer object for adjusting limit information.
/// </summary>
public sealed class AdjustLimitDto
{
    /// <summary>
    /// Gets or sets the adjustment amount (positive for increase, negative for decrease).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the description of the adjustment.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for updating limit information (backward compatibility).
/// </summary>
public sealed class UpdateLimitDto
{
    /// <summary>
    /// Gets or sets the limit amount.
    /// </summary>
    public decimal LimitAmount { get; set; }

    /// <summary>
    /// Gets or sets the limit description.
    /// </summary>
    public string LimitDescription { get; set; } = "Credit Limit";
}

/// <summary>
/// Data transfer object for paginated customer results.
/// </summary>
public sealed class PagedCustomersDto
{
    /// <summary>
    /// Gets or sets the list of customers.
    /// </summary>
    public List<CustomerDto> Customers { get; set; } = [];

    /// <summary>
    /// Gets or sets the next page key for pagination.
    /// </summary>
    public string? NextPageKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are more pages.
    /// </summary>
    public bool HasNextPage { get; set; }
}
