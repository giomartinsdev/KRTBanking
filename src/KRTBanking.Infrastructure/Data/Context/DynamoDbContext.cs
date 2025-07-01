using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KRTBanking.Infrastructure.Data.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRTBanking.Infrastructure.Data.Context;

/// <summary>
/// DynamoDB context for managing database operations and table setup.
/// </summary>
public sealed class DynamoDbContext
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDbOptions _options;
    private readonly ILogger<DynamoDbContext> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamoDbContext"/> class.
    /// </summary>
    /// <param name="dynamoDbClient">The DynamoDB client.</param>
    /// <param name="options">The DynamoDB options.</param>
    /// <param name="logger">The logger.</param>
    public DynamoDbContext(
        IAmazonDynamoDB dynamoDbClient,
        IOptions<DynamoDbOptions> options,
        ILogger<DynamoDbContext> logger)
    {
        _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ensures that the required DynamoDB tables exist, creating them if necessary.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureTablesExistAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCustomerTableExistsAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures that the customer table exists, creating it if necessary.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureCustomerTableExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var listTablesResponse = await _dynamoDbClient.ListTablesAsync(cancellationToken);
            if (listTablesResponse.TableNames.Contains(_options.CustomerTableName))
            {
                _logger.LogInformation("Customer table {TableName} already exists", _options.CustomerTableName);
                return;
            }

            _logger.LogInformation("Creating customer table {TableName}", _options.CustomerTableName);

            var createTableRequest = new CreateTableRequest
            {
                TableName = _options.CustomerTableName,
                KeySchema =
                [
                    new KeySchemaElement
                    {
                        AttributeName = "PK",
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "SK",
                        KeyType = KeyType.RANGE
                    }
                ],
                AttributeDefinitions =
                [
                    new AttributeDefinition
                    {
                        AttributeName = "PK",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "SK",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "GSI1PK",
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "GSI1SK",
                        AttributeType = ScalarAttributeType.S
                    }
                ],
                GlobalSecondaryIndexes =
                [
                    new GlobalSecondaryIndex
                    {
                        IndexName = "GSI1",
                        KeySchema =
                        [
                            new KeySchemaElement
                            {
                                AttributeName = "GSI1PK",
                                KeyType = KeyType.HASH
                            },
                            new KeySchemaElement
                            {
                                AttributeName = "GSI1SK",
                                KeyType = KeyType.RANGE
                            }
                        ],
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        }
                    }
                ],
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            await _dynamoDbClient.CreateTableAsync(createTableRequest, cancellationToken);

            await WaitForTableToBeActiveAsync(_options.CustomerTableName, cancellationToken);

            _logger.LogInformation("Customer table {TableName} created successfully", _options.CustomerTableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer table {TableName}", _options.CustomerTableName);
            throw;
        }
    }

    /// <summary>
    /// Waits for a DynamoDB table to become active.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task WaitForTableToBeActiveAsync(string tableName, CancellationToken cancellationToken)
    {
        var maxAttempts = 30;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            try
            {
                var response = await _dynamoDbClient.DescribeTableAsync(tableName, cancellationToken);
                
                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    _logger.LogInformation("Table {TableName} is now active", tableName);
                    return;
                }

                _logger.LogInformation("Table {TableName} status: {Status}. Waiting...", 
                    tableName, response.Table.TableStatus);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                attempt++;
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogWarning("Table {TableName} not found during status check", tableName);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                attempt++;
            }
        }

        throw new TimeoutException($"Table {tableName} did not become active within the expected time.");
    }
}
