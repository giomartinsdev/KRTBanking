using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Configuration;
using KRTBanking.Infrastructure.Data.Repositories;
using KRTBanking.Infrastructure.Tests.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace KRTBanking.Infrastructure.Tests.Data.Repositories;

/// <summary>
/// Tests for CustomerRepository using AAA pattern and mocked DynamoDB.
/// Tests cover all repository methods without requiring a real database connection.
/// </summary>
public class CustomerRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoClient;
    private readonly Mock<ILogger<CustomerRepository>> _mockLogger;
    private readonly IOptions<DynamoDbOptions> _options;
    private readonly CustomerRepository _customerRepository;

    public CustomerRepositoryTests()
    {
        // Arrange - Common setup for all tests
        _mockDynamoClient = new Mock<IAmazonDynamoDB>();
        _mockLogger = new Mock<ILogger<CustomerRepository>>();
        _options = InfrastructureTestBuilders.DynamoDbOptions().Build();
        
        _customerRepository = new CustomerRepository(
            _mockDynamoClient.Object,
            _options,
            _mockLogger.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedItem = InfrastructureTestBuilders.ADynamoDbItem()
            .WithCustomerAttributes(customerId, "11144477735", "John Doe", "john@email.com")
            .Build();

        var getItemResponse = new GetItemResponse
        {
            Item = expectedItem,
            IsItemSet = true
        };

        _mockDynamoClient.Setup(x => x.GetItemAsync(
                It.Is<GetItemRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.Key["PK"].S.Contains(customerId.ToString())),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(getItemResponse);

        // Act
        var result = await _customerRepository.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.DocumentNumber.Value.Should().Be("11144477735");
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@email.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var getItemResponse = new GetItemResponse { IsItemSet = false };

        _mockDynamoClient.Setup(x => x.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(getItemResponse);

        // Act
        var result = await _customerRepository.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenDynamoDbThrowsException_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedException = new Exception("DynamoDB error");

        _mockDynamoClient.Setup(x => x.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _customerRepository.GetByIdAsync(customerId));
        
        exception.Should().Be(expectedException);
    }

    #endregion

    #region GetByDocumentNumberAsync Tests

    [Fact]
    public async Task GetByDocumentNumberAsync_WithDocumentNumberObject_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var customerId = Guid.NewGuid();
        var expectedItem = InfrastructureTestBuilders.ADynamoDbItem()
            .WithCustomerAttributes(customerId, documentNumber.Value, "Jane Doe", "jane@email.com")
            .Build();

        var queryResponse = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>> { expectedItem }
        };

        _mockDynamoClient.Setup(x => x.QueryAsync(
                It.Is<QueryRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.IndexName == "GSI1" &&
                    r.ExpressionAttributeValues.ContainsKey(":gsi1pk")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var result = await _customerRepository.GetByDocumentNumberAsync(documentNumber);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentNumber.Value.Should().Be(documentNumber.Value);
        result.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithDocumentNumberObject_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var documentNumber = new DocumentNumber("52998224725");
        var queryResponse = new QueryResponse { Items = new List<Dictionary<string, AttributeValue>>() };

        _mockDynamoClient.Setup(x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var result = await _customerRepository.GetByDocumentNumberAsync(documentNumber);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithStringDocumentNumber_WhenValidDocumentNumber_ShouldReturnCustomer()
    {
        // Arrange
        var documentNumberString = "11144477735";
        var customerId = Guid.NewGuid();
        var expectedItem = InfrastructureTestBuilders.ADynamoDbItem()
            .WithCustomerAttributes(customerId, documentNumberString, "Bob Smith", "bob@email.com")
            .Build();

        var queryResponse = new QueryResponse
        {
            Items = new List<Dictionary<string, AttributeValue>> { expectedItem }
        };

        _mockDynamoClient.Setup(x => x.QueryAsync(
                It.IsAny<QueryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var result = await _customerRepository.GetByDocumentNumberAsync(documentNumberString);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentNumber.Value.Should().Be(documentNumberString);
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithStringDocumentNumber_WhenInvalidDocumentNumber_ShouldReturnNull()
    {
        // Arrange
        var invalidDocumentNumber = "invalid";

        // Act
        var result = await _customerRepository.GetByDocumentNumberAsync(invalidDocumentNumber);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithStringDocumentNumber_WhenNullOrEmpty_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _customerRepository.GetByDocumentNumberAsync(string.Empty));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => _customerRepository.GetByDocumentNumberAsync("   "));
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithDocumentNumberObject_WhenNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _customerRepository.GetByDocumentNumberAsync((DocumentNumber)null!));
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WhenCustomerIsValid_ShouldAddSuccessfully()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer()
            .WithDocumentNumber("11144477735")
            .WithName("New Customer")
            .WithEmail("new@email.com")
            .Build();

        var putItemResponse = new PutItemResponse();

        _mockDynamoClient.Setup(x => x.PutItemAsync(
                It.Is<PutItemRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.ConditionExpression == "attribute_not_exists(PK)"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(putItemResponse);

        // Act
        await _customerRepository.AddAsync(customer);

        // Assert
        _mockDynamoClient.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(r => r.TableName == _options.Value.CustomerTableName),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenCustomerAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer().Build();
        var conditionalCheckException = new ConditionalCheckFailedException("Customer already exists");

        _mockDynamoClient.Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(conditionalCheckException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerRepository.AddAsync(customer));
        
        exception.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task AddAsync_WhenCustomerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _customerRepository.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_WhenDynamoDbThrowsException_ShouldThrowException()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer().Build();
        var expectedException = new Exception("DynamoDB error");

        _mockDynamoClient.Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _customerRepository.AddAsync(customer));
        
        exception.Should().Be(expectedException);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenCustomerIsValid_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer()
            .WithVersion(2)
            .WithName("Updated Customer")
            .Build();

        var putItemResponse = new PutItemResponse();

        _mockDynamoClient.Setup(x => x.PutItemAsync(
                It.Is<PutItemRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.ConditionExpression.Contains("attribute_exists(PK)") &&
                    r.ConditionExpression.Contains("#version = :expectedVersion")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(putItemResponse);

        // Act
        await _customerRepository.UpdateAsync(customer);

        // Assert
        _mockDynamoClient.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(r => 
                r.TableName == _options.Value.CustomerTableName &&
                r.ExpressionAttributeValues[":expectedVersion"].N == "1"), // Version - 1
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenConcurrencyConflict_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer().WithVersion(2).Build();
        var conditionalCheckException = new ConditionalCheckFailedException("Concurrency conflict");

        _mockDynamoClient.Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(conditionalCheckException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerRepository.UpdateAsync(customer));
        
        exception.Message.Should().Contain("modified by another process");
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _customerRepository.UpdateAsync(null!));
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WhenCustomerExists_ShouldRemoveSuccessfully()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer().Build();
        var deleteItemResponse = new DeleteItemResponse();

        _mockDynamoClient.Setup(x => x.DeleteItemAsync(
                It.Is<DeleteItemRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.ConditionExpression == "attribute_exists(PK)"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteItemResponse);

        // Act
        await _customerRepository.RemoveAsync(customer);

        // Assert
        _mockDynamoClient.Verify(x => x.DeleteItemAsync(
            It.Is<DeleteItemRequest>(r => 
                r.TableName == _options.Value.CustomerTableName &&
                r.Key["PK"].S.Contains(customer.Id.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenCustomerDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer().Build();
        var conditionalCheckException = new ConditionalCheckFailedException("Customer not found");

        _mockDynamoClient.Setup(x => x.DeleteItemAsync(
                It.IsAny<DeleteItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(conditionalCheckException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _customerRepository.RemoveAsync(customer));
        
        exception.Message.Should().Contain("does not exist");
    }

    [Fact]
    public async Task RemoveAsync_WhenCustomerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _customerRepository.RemoveAsync(null!));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithDefaultParameters_ShouldReturnActiveCustomersOnly()
    {
        // Arrange
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();
        
        var scanResponse = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                InfrastructureTestBuilders.ADynamoDbItem()
                    .WithCustomerAttributes(customer1Id, "11144477735", "Customer 1", "customer1@email.com")
                    .Build(),
                InfrastructureTestBuilders.ADynamoDbItem()
                    .WithCustomerAttributes(customer2Id, "52998224725", "Customer 2", "customer2@email.com")
                    .Build()
            }
        };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.Is<ScanRequest>(r => 
                    r.TableName == _options.Value.CustomerTableName &&
                    r.FilterExpression.Contains("IsActive = :isActive") &&
                    r.Limit == 50),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        var (customers, nextPageKey) = await _customerRepository.GetAllAsync(pageSize: 50, lastEvaluatedKey: null, includeInactive: false);

        // Assert
        customers.Should().HaveCount(2);
        customers.Should().OnlyContain(c => c.IsActive);
        nextPageKey.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeInactiveTrue_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();
        
        var scanResponse = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                InfrastructureTestBuilders.ADynamoDbItem()
                    .WithCustomerAttributes(customer1Id, "11144477735", "Active Customer", "active@email.com")
                    .WithAttribute("IsActive", true)
                    .Build(),
                InfrastructureTestBuilders.ADynamoDbItem()
                    .WithCustomerAttributes(customer2Id, "52998224725", "Inactive Customer", "inactive@email.com")
                    .WithAttribute("IsActive", false)
                    .Build()
            }
        };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.Is<ScanRequest>(r => 
                    r.FilterExpression == "SK = :sk" &&
                    !r.FilterExpression.Contains("IsActive")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        var (customers, nextPageKey) = await _customerRepository.GetAllAsync(
            pageSize: 50, 
            includeInactive: true);

        // Assert
        customers.Should().HaveCount(2);
        customers.Should().Contain(c => c.IsActive);
        customers.Should().Contain(c => !c.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_WithCustomPageSize_ShouldRespectPageSize()
    {
        // Arrange
        var pageSize = 10;
        var scanResponse = new ScanResponse { Items = new List<Dictionary<string, AttributeValue>>() };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.Is<ScanRequest>(r => r.Limit == pageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        await _customerRepository.GetAllAsync(pageSize: pageSize, lastEvaluatedKey: null, includeInactive: false);

        // Assert
        _mockDynamoClient.Verify(x => x.ScanAsync(
            It.Is<ScanRequest>(r => r.Limit == pageSize),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithLastEvaluatedKey_ShouldIncludeExclusiveStartKey()
    {
        // Arrange
        var lastKey = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "CUSTOMER#123" },
            ["SK"] = new AttributeValue { S = "CUSTOMER" }
        };
        var lastKeyJson = JsonSerializer.Serialize(lastKey);
        var lastKeyBytes = Encoding.UTF8.GetBytes(lastKeyJson);
        var lastEvaluatedKey = Convert.ToBase64String(lastKeyBytes);

        var scanResponse = new ScanResponse { Items = new List<Dictionary<string, AttributeValue>>() };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.Is<ScanRequest>(r => r.ExclusiveStartKey != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        await _customerRepository.GetAllAsync(pageSize: 50, lastEvaluatedKey: lastEvaluatedKey, includeInactive: false);

        // Assert
        _mockDynamoClient.Verify(x => x.ScanAsync(
            It.Is<ScanRequest>(r => r.ExclusiveStartKey != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidLastEvaluatedKey_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidKey = "invalid_base64_key";

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _customerRepository.GetAllAsync(pageSize: 50, lastEvaluatedKey: invalidKey, includeInactive: false));
        
        exception.ParamName.Should().Be("lastEvaluatedKey");
    }

    [Fact]
    public async Task GetAllAsync_WhenHasMoreResults_ShouldReturnNextPageKey()
    {
        // Arrange
        var lastEvaluatedKey = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "CUSTOMER#123" },
            ["SK"] = new AttributeValue { S = "CUSTOMER" }
        };

        var scanResponse = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>(),
            LastEvaluatedKey = lastEvaluatedKey
        };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.IsAny<ScanRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        var (customers, nextPageKey) = await _customerRepository.GetAllAsync(pageSize: 50, lastEvaluatedKey: null, includeInactive: false);

        // Assert
        nextPageKey.Should().NotBeNull();
        nextPageKey.Should().NotBeEmpty();
    }

    #endregion

    #region GetAllActiveAsync Tests

    [Fact]
    public async Task GetAllActiveAsync_ShouldCallGetAllAsyncWithIncludeInactiveFalse()
    {
        // Arrange
        var scanResponse = new ScanResponse { Items = new List<Dictionary<string, AttributeValue>>() };

        _mockDynamoClient.Setup(x => x.ScanAsync(
                It.Is<ScanRequest>(r => 
                    r.FilterExpression.Contains("IsActive = :isActive")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(scanResponse);

        // Act
        await _customerRepository.GetAllActiveAsync();

        // Assert
        _mockDynamoClient.Verify(x => x.ScanAsync(
            It.Is<ScanRequest>(r => 
                r.FilterExpression.Contains("IsActive = :isActive") &&
                r.ExpressionAttributeValues[":isActive"].BOOL == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenDynamoClientIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CustomerRepository(null!, _options, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CustomerRepository(_mockDynamoClient.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CustomerRepository(_mockDynamoClient.Object, _options, null!));
    }

    [Fact]
    public void Constructor_WhenAllParametersAreValid_ShouldCreateInstance()
    {
        // Arrange & Act
        var repository = new CustomerRepository(_mockDynamoClient.Object, _options, _mockLogger.Object);

        // Assert
        repository.Should().NotBeNull();
    }

    #endregion
}
