using System.Text.Json;
using Amazon.DynamoDBv2.Model;

namespace KRTBanking.Infrastructure.Data.Models;

/// <summary>
/// DynamoDB representation of a customer entity.
/// </summary>
public sealed class CustomerDynamoDbModel
{
    /// <summary>
    /// Gets or sets the partition key (customer ID).
    /// </summary>
    public string PK { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort key (customer type).
    /// </summary>
    public string SK { get; set; } = "CUSTOMER";

    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account information as JSON.
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the limits as JSON.
    /// </summary>
    public string Limits { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version for optimistic concurrency control.
    /// </summary>
    public long Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the customer is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the Global Secondary Index 1 partition key (document number).
    /// </summary>
    public string GSI1PK { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Global Secondary Index 1 sort key.
    /// </summary>
    public string GSI1SK { get; set; } = "CUSTOMER";

    /// <summary>
    /// Converts the model to DynamoDB attribute values.
    /// </summary>
    /// <returns>A dictionary of attribute values.</returns>
    public Dictionary<string, AttributeValue> ToDynamoDbItem()
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = PK },
            ["SK"] = new AttributeValue { S = SK },
            ["CustomerId"] = new AttributeValue { S = CustomerId },
            ["DocumentNumber"] = new AttributeValue { S = DocumentNumber },
            ["Name"] = new AttributeValue { S = Name },
            ["Email"] = new AttributeValue { S = Email },
            ["Limits"] = new AttributeValue { S = Limits },
            ["CreatedAt"] = new AttributeValue { S = CreatedAt },
            ["UpdatedAt"] = new AttributeValue { S = UpdatedAt },
            ["Version"] = new AttributeValue { N = Version.ToString() },
            ["IsActive"] = new AttributeValue { BOOL = IsActive },
            ["GSI1PK"] = new AttributeValue { S = GSI1PK },
            ["GSI1SK"] = new AttributeValue { S = GSI1SK }
        };

        if (!string.IsNullOrEmpty(Account))
        {
            item["Account"] = new AttributeValue { S = Account };
        }

        return item;
    }

    /// <summary>
    /// Creates a model from DynamoDB attribute values.
    /// </summary>
    /// <param name="item">The DynamoDB item.</param>
    /// <returns>A new <see cref="CustomerDynamoDbModel"/> instance.</returns>
    public static CustomerDynamoDbModel FromDynamoDbItem(Dictionary<string, AttributeValue> item)
    {
        return new CustomerDynamoDbModel
        {
            PK = item.GetValueOrDefault("PK")?.S ?? string.Empty,
            SK = item.GetValueOrDefault("SK")?.S ?? string.Empty,
            CustomerId = item.GetValueOrDefault("CustomerId")?.S ?? string.Empty,
            DocumentNumber = item.GetValueOrDefault("DocumentNumber")?.S ?? string.Empty,
            Name = item.GetValueOrDefault("Name")?.S ?? string.Empty,
            Email = item.GetValueOrDefault("Email")?.S ?? string.Empty,
            Account = item.GetValueOrDefault("Account")?.S,
            Limits = item.GetValueOrDefault("Limits")?.S ?? "[]",
            CreatedAt = item.GetValueOrDefault("CreatedAt")?.S ?? string.Empty,
            UpdatedAt = item.GetValueOrDefault("UpdatedAt")?.S ?? string.Empty,
            Version = long.TryParse(item.GetValueOrDefault("Version")?.N, out var version) ? version : 1,
            IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true,
            GSI1PK = item.GetValueOrDefault("GSI1PK")?.S ?? string.Empty,
            GSI1SK = item.GetValueOrDefault("GSI1SK")?.S ?? string.Empty
        };
    }

    /// <summary>
    /// Creates the partition key for a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>The partition key.</returns>
    public static string CreatePartitionKey(Guid customerId)
    {
        return $"CUSTOMER#{customerId}";
    }

    /// <summary>
    /// Creates the GSI1 partition key for document number lookup.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <returns>The GSI1 partition key.</returns>
    public static string CreateGsi1PartitionKey(string documentNumber)
    {
        return $"DOC#{documentNumber}";
    }
}
