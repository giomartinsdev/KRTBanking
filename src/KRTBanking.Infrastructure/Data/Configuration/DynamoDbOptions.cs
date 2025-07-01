namespace KRTBanking.Infrastructure.Data.Configuration;

/// <summary>
/// Configuration options for DynamoDB.
/// </summary>
public sealed class DynamoDbOptions
{
    /// <summary>
    /// Gets or sets the DynamoDB service URL.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the AWS access key.
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Gets or sets the AWS secret key.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the AWS region.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Gets or sets the customer table name.
    /// </summary>
    public string CustomerTableName { get; set; } = "Customers";

    /// <summary>
    /// Gets or sets a value indicating whether to use local DynamoDB.
    /// </summary>
    public bool UseLocalDb { get; set; } = false;
}
