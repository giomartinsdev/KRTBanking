using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KRTBanking.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace KRTBanking.Infrastructure.Data;

/// <summary>
/// DynamoDB implementation of database initialization.
/// Follows SRP by handling only DynamoDB table creation concerns.
/// </summary>
public class DynamoDbInitializer : IDatabaseInitializer
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<DynamoDbInitializer> _logger;

    public DynamoDbInitializer(IAmazonDynamoDB dynamoDbClient, ILogger<DynamoDbInitializer> logger)
    {
        _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes all required DynamoDB tables for the KRT Banking system.
    /// Implements proper async patterns and error handling.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting DynamoDB initialization process...");
        
        await ValidateDynamoDbConnectionAsync(cancellationToken);
        
        var tables = GetRequiredTables();
        _logger.LogInformation("Found {TableCount} tables to initialize", tables.Count);

        foreach (var tableConfig in tables)
        {
            try
            {
                _logger.LogInformation("Initializing table: {TableName}", tableConfig.TableName);
                await CreateTableIfNotExistsAsync(tableConfig, cancellationToken);
                _logger.LogInformation("Successfully initialized table: {TableName}", tableConfig.TableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize table: {TableName}. Database initialization cannot continue.", tableConfig.TableName);
                throw new InvalidOperationException($"Database initialization failed for table {tableConfig.TableName}. See inner exception for details.", ex);
            }
        }
        
        _logger.LogInformation("DynamoDB initialization completed successfully. All {TableCount} tables are ready.", tables.Count);
    }

    /// <summary>
    /// Validates that we can connect to DynamoDB before attempting table operations.
    /// </summary>
    private async Task ValidateDynamoDbConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Validating DynamoDB connection...");
            await _dynamoDbClient.ListTablesAsync(new ListTablesRequest { Limit = 1 }, cancellationToken);
            _logger.LogDebug("DynamoDB connection validated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to DynamoDB. Please verify your connection configuration.");
            throw new InvalidOperationException("Cannot connect to DynamoDB. Please verify your connection configuration.", ex);
        }
    }

    /// <summary>
    /// Defines the required tables for the KRT Banking domain.
    /// Encapsulates table configuration knowledge.
    /// </summary>
    private static List<TableConfiguration> GetRequiredTables()
    {
        return new List<TableConfiguration>
        {
            new TableConfiguration
            {
                TableName = "KRTBanking-Customers",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Id", KeyType.HASH)
                },
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5
            }
        };
    }

    /// <summary>
    /// Creates a DynamoDB table if it doesn't already exist.
    /// Implements proper async patterns with cancellation support.
    /// </summary>
    private async Task CreateTableIfNotExistsAsync(TableConfiguration config, CancellationToken cancellationToken)
    {
        try
        {
            await _dynamoDbClient.DescribeTableAsync(config.TableName, cancellationToken);
            _logger.LogDebug("Table {TableName} already exists", config.TableName);
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogInformation("Creating table: {TableName}", config.TableName);
            
            var createTableRequest = new CreateTableRequest
            {
                TableName = config.TableName,
                AttributeDefinitions = config.AttributeDefinitions,
                KeySchema = config.KeySchema,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = config.ReadCapacityUnits,
                    WriteCapacityUnits = config.WriteCapacityUnits
                }
            };

            await _dynamoDbClient.CreateTableAsync(createTableRequest, cancellationToken);
            await WaitForTableToBeActiveAsync(config.TableName, cancellationToken);
        }
    }

    /// <summary>
    /// Waits for a DynamoDB table to become active after creation.
    /// Implements proper polling with cancellation support and timeout.
    /// </summary>
    private async Task WaitForTableToBeActiveAsync(string tableName, CancellationToken cancellationToken)
    {
        const int maxWaitTimeSeconds = 60;
        const int pollIntervalSeconds = 2;
        
        var startTime = DateTime.UtcNow;
        var tableStatus = "CREATING";
        
        _logger.LogInformation("Waiting for table {TableName} to become active...", tableName);
        
        while (tableStatus == "CREATING" && !cancellationToken.IsCancellationRequested)
        {
            // Check timeout
            if ((DateTime.UtcNow - startTime).TotalSeconds > maxWaitTimeSeconds)
            {
                throw new TimeoutException($"Table {tableName} creation timed out after {maxWaitTimeSeconds} seconds. Current status: {tableStatus}");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), cancellationToken);
            
            try
            {
                var response = await _dynamoDbClient.DescribeTableAsync(tableName, cancellationToken);
                tableStatus = response.Table.TableStatus;
                
                _logger.LogDebug("Table {TableName} status: {Status} (elapsed: {ElapsedSeconds}s)", 
                    tableName, tableStatus, (int)(DateTime.UtcNow - startTime).TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking table {TableName} status, will retry...", tableName);
            }
        }
        
        cancellationToken.ThrowIfCancellationRequested();

        if (tableStatus != "ACTIVE")
        {
            throw new InvalidOperationException($"Table {tableName} failed to become active. Current status: {tableStatus}");
        }

        _logger.LogInformation("Table {TableName} is now active (total time: {ElapsedSeconds}s)", 
            tableName, (int)(DateTime.UtcNow - startTime).TotalSeconds);
    }
}

/// <summary>
/// Configuration class for DynamoDB table creation.
/// Follows value object principles for encapsulating table configuration.
/// </summary>
internal class TableConfiguration
{
    public string TableName { get; set; } = string.Empty;
    public List<AttributeDefinition> AttributeDefinitions { get; set; } = new();
    public List<KeySchemaElement> KeySchema { get; set; } = new();
    public long ReadCapacityUnits { get; set; }
    public long WriteCapacityUnits { get; set; }
}
