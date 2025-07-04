using Amazon.DynamoDBv2.DataModel;

namespace KRTBanking.Infrastructure.Data.Models;

/// <summary>
/// DynamoDB model for Customer entity following the article pattern.
/// </summary>
[DynamoDBTable("KRTBanking-Customers")]
public sealed class CustomerDynamoModel
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    [DynamoDBHashKey]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's document number.
    /// </summary>
    [DynamoDBProperty]
    public required string DocumentNumber { get; set; }

    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    [DynamoDBProperty]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    [DynamoDBProperty]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    [DynamoDBProperty]
    public required string AccountNumber { get; set; }

    /// <summary>
    /// Gets or sets the account creation timestamp.
    /// </summary>
    [DynamoDBProperty]
    public DateTime AccountCreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the limit entries as JSON string.
    /// </summary>
    [DynamoDBProperty]
    public string LimitEntries { get; set; } = "[]";

    /// <summary>
    /// Gets or sets a value indicating whether the customer is active.
    /// </summary>
    [DynamoDBProperty]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the version for optimistic concurrency control.
    /// </summary>
    [DynamoDBProperty]
    public long Version { get; set; }
}
