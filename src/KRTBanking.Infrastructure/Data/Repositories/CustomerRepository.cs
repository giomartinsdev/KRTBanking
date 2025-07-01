using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.Repositories;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Configuration;
using KRTBanking.Infrastructure.Data.Mappers;
using KRTBanking.Infrastructure.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRTBanking.Infrastructure.Data.Repositories;

/// <summary>
/// DynamoDB implementation of the customer repository.
/// </summary>
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDbOptions _options;
    private readonly ILogger<CustomerRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
    /// </summary>
    /// <param name="dynamoDbClient">The DynamoDB client.</param>
    /// <param name="options">The DynamoDB options.</param>
    /// <param name="logger">The logger.</param>
    public CustomerRepository(
        IAmazonDynamoDB dynamoDbClient,
        IOptions<DynamoDbOptions> options,
        ILogger<CustomerRepository> logger)
    {
        _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = _options.CustomerTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["PK"] = new AttributeValue { S = CustomerDynamoDbModel.CreatePartitionKey(customerId) },
                    ["SK"] = new AttributeValue { S = "CUSTOMER" }
                }
            };

            var response = await _dynamoDbClient.GetItemAsync(request, cancellationToken);

            if (!response.IsItemSet)
            {
                _logger.LogInformation("Customer with ID {CustomerId} not found", customerId);
                return null;
            }

            var model = CustomerDynamoDbModel.FromDynamoDbItem(response.Item);
            return CustomerMapper.ToDomain(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", customerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByDocumentNumberAsync(
        DocumentNumber documentNumber, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNumber);

        try
        {
            var request = new QueryRequest
            {
                TableName = _options.CustomerTableName,
                IndexName = "GSI1",
                KeyConditionExpression = "GSI1PK = :gsi1pk AND GSI1SK = :gsi1sk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":gsi1pk"] = new AttributeValue { S = CustomerDynamoDbModel.CreateGsi1PartitionKey(documentNumber.Value) },
                    [":gsi1sk"] = new AttributeValue { S = "CUSTOMER" }
                },
                Limit = 1
            };

            var response = await _dynamoDbClient.QueryAsync(request, cancellationToken);

            if (response.Items.Count == 0)
            {
                _logger.LogInformation("Customer with document number {DocumentNumber} not found", documentNumber.Value);
                return null;
            }

            var model = CustomerDynamoDbModel.FromDynamoDbItem(response.Items[0]);
            return CustomerMapper.ToDomain(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with document number {DocumentNumber}", documentNumber.Value);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByDocumentNumberAsync(
        string documentNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            throw new ArgumentException("Document number cannot be null or empty.", nameof(documentNumber));
        }

        var documentNumberValue = DocumentNumber.TryCreate(documentNumber);
        if (documentNumberValue is null)
        {
            _logger.LogWarning("Invalid document number format: {DocumentNumber}", documentNumber);
            return null;
        }

        return await GetByDocumentNumberAsync(documentNumberValue, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        try
        {
            var model = CustomerMapper.ToModel(customer);
            var request = new PutItemRequest
            {
                TableName = _options.CustomerTableName,
                Item = model.ToDynamoDbItem(),
                ConditionExpression = "attribute_not_exists(PK)"
            };

            await _dynamoDbClient.PutItemAsync(request, cancellationToken);
            _logger.LogInformation("Customer {CustomerId} added successfully", customer.Id);
        }
        catch (ConditionalCheckFailedException)
        {
            _logger.LogWarning("Customer {CustomerId} already exists", customer.Id);
            throw new InvalidOperationException($"Customer with ID {customer.Id} already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customer {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        try
        {
            var model = CustomerMapper.ToModel(customer);
            var request = new PutItemRequest
            {
                TableName = _options.CustomerTableName,
                Item = model.ToDynamoDbItem(),
                ConditionExpression = "attribute_exists(PK) AND #version = :expectedVersion",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#version"] = "Version"
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":expectedVersion"] = new AttributeValue { N = (customer.Version - 1).ToString() }
                }
            };

            await _dynamoDbClient.PutItemAsync(request, cancellationToken);
            _logger.LogInformation("Customer {CustomerId} updated successfully", customer.Id);
        }
        catch (ConditionalCheckFailedException)
        {
            _logger.LogWarning("Optimistic concurrency conflict for customer {CustomerId}", customer.Id);
            throw new InvalidOperationException($"Customer with ID {customer.Id} has been modified by another process.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        try
        {
            var request = new DeleteItemRequest
            {
                TableName = _options.CustomerTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["PK"] = new AttributeValue { S = CustomerDynamoDbModel.CreatePartitionKey(customer.Id) },
                    ["SK"] = new AttributeValue { S = "CUSTOMER" }
                },
                ConditionExpression = "attribute_exists(PK)"
            };

            await _dynamoDbClient.DeleteItemAsync(request, cancellationToken);
            _logger.LogInformation("Customer {CustomerId} removed successfully", customer.Id);
        }
        catch (ConditionalCheckFailedException)
        {
            _logger.LogWarning("Customer {CustomerId} not found for deletion", customer.Id);
            throw new InvalidOperationException($"Customer with ID {customer.Id} does not exist.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing customer {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Customer> customers, string? nextPageKey)> GetAllAsync(
        int pageSize = 50, 
        string? lastEvaluatedKey = null, 
        CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(pageSize, lastEvaluatedKey, includeInactive: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Customer> customers, string? nextPageKey)> GetAllAsync(
        int pageSize = 50, 
        string? lastEvaluatedKey = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ScanRequest
            {
                TableName = _options.CustomerTableName,
                Limit = pageSize
            };

            if (includeInactive)
            {
                request.FilterExpression = "SK = :sk";
                request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":sk"] = new AttributeValue { S = "CUSTOMER" }
                };
            }
            else
            {
                request.FilterExpression = "SK = :sk AND IsActive = :isActive";
                request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":sk"] = new AttributeValue { S = "CUSTOMER" },
                    [":isActive"] = new AttributeValue { BOOL = true }
                };
            }

            if (!string.IsNullOrEmpty(lastEvaluatedKey))
            {
                try
                {
                    var keyBytes = Convert.FromBase64String(lastEvaluatedKey);
                    var keyJson = Encoding.UTF8.GetString(keyBytes);
                    var lastKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(keyJson);
                    request.ExclusiveStartKey = lastKey;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid pagination key provided: {Key}", lastEvaluatedKey);
                    throw new ArgumentException("Invalid pagination key.", nameof(lastEvaluatedKey));
                }
            }

            var response = await _dynamoDbClient.ScanAsync(request, cancellationToken);
            var customers = new List<Customer>();

            foreach (var item in response.Items)
            {
                var model = CustomerDynamoDbModel.FromDynamoDbItem(item);
                customers.Add(CustomerMapper.ToDomain(model));
            }

            string? nextPageKey = null;
            if (response.LastEvaluatedKey?.Count > 0)
            {
                var keyJson = JsonSerializer.Serialize(response.LastEvaluatedKey);
                var keyBytes = Encoding.UTF8.GetBytes(keyJson);
                nextPageKey = Convert.ToBase64String(keyBytes);
            }

            _logger.LogInformation("Retrieved {Count} customers (includeInactive: {IncludeInactive})", customers.Count, includeInactive);
            return (customers, nextPageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Customer> customers, string? nextPageKey)> GetAllActiveAsync(
        int pageSize = 50, 
        string? lastEvaluatedKey = null, 
        CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(pageSize, lastEvaluatedKey, includeInactive: false, cancellationToken);
    }
}
