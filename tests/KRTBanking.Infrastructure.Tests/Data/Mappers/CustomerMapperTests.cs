using FluentAssertions;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Mappers;
using KRTBanking.Infrastructure.Data.Models;
using KRTBanking.Infrastructure.Tests.Builders;
using System.Text.Json;
using Xunit;

namespace KRTBanking.Infrastructure.Tests.Data.Mappers;

/// <summary>
/// Tests for CustomerMapper using AAA pattern.
/// Tests cover mapping between domain entities and DynamoDB models.
/// </summary>
public class CustomerMapperTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    #region ToModel Tests

    [Fact]
    public void ToModel_WhenCustomerIsValid_ShouldMapCorrectly()
    {
        // Arrange
        var account = InfrastructureTestBuilders.AnAccount()
            .WithNumber("123456")
            .WithAgency(Agency.Agency1)
            .WithBalance(1500.50m)
            .Build();

        var limit = InfrastructureTestBuilders.ALimitEntry()
            .WithAmount(1000.00m)
            .WithDescription("Daily Withdrawal Limit")
            .Build();

        var customer = InfrastructureTestBuilders.ACustomer()
            .WithId(Guid.NewGuid())
            .WithDocumentNumber("11144477735")
            .WithName("John Doe")
            .WithEmail("john.doe@email.com")
            .WithAccount(account)
            .WithLimitEntries(limit)
            .WithVersion(2)
            .WithIsActive(true)
            .Build();

        // Act
        var result = CustomerMapper.ToModel(customer);

        // Assert
        result.Should().NotBeNull();
        result.PK.Should().Be(CustomerDynamoDbModel.CreatePartitionKey(customer.Id));
        result.SK.Should().Be("CUSTOMER");
        result.CustomerId.Should().Be(customer.Id.ToString());
        result.DocumentNumber.Should().Be(customer.DocumentNumber.Value);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
        result.Version.Should().Be(customer.Version);
        result.IsActive.Should().Be(customer.IsActive);
        result.GSI1PK.Should().Be(CustomerDynamoDbModel.CreateGsi1PartitionKey(customer.DocumentNumber.Value));
        result.GSI1SK.Should().Be("CUSTOMER");
        
        result.Account.Should().NotBeEmpty();
        result.Limits.Should().NotBeEmpty();
        
        DateTime.TryParse(result.CreatedAt, out _).Should().BeTrue();
        DateTime.TryParse(result.UpdatedAt, out _).Should().BeTrue();
    }



    [Fact]
    public void ToModel_WhenCustomerHasInitialLimitOnly_ShouldMapCorrectly()
    {
        // Arrange
        var customer = InfrastructureTestBuilders.ACustomer()
            .WithDocumentNumber("52998224725")
            .WithName("Initial Limit Customer")
            .WithEmail("initiallimit@email.com")
            .Build();

        // Act
        var result = CustomerMapper.ToModel(customer);

        // Assert
        result.Should().NotBeNull();
        result.Limits.Should().NotBeEmpty();
        result.Name.Should().Be("Initial Limit Customer");
        
        var deserializedLimits = JsonSerializer.Deserialize<List<object>>(result.Limits, JsonOptions);
        deserializedLimits.Should().HaveCount(1); // Only the initial limit
    }

    [Fact]
    public void ToModel_WhenCustomerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => CustomerMapper.ToModel(null!));
    }

    [Fact]
    public void ToModel_WhenCustomerHasMultipleLimits_ShouldMapAllLimits()
    {
        // Arrange
        var dailyLimit = InfrastructureTestBuilders.ALimitEntry()
            .WithAmount(1000.00m)
            .WithDescription("Daily Limit")
            .Build();

        var monthlyLimit = InfrastructureTestBuilders.ALimitEntry()
            .WithAmount(5000.00m)
            .WithDescription("Monthly Limit")
            .Build();

        var customer = InfrastructureTestBuilders.ACustomer()
            .WithLimitEntries(dailyLimit, monthlyLimit)
            .Build();

        // Act
        var result = CustomerMapper.ToModel(customer);

        // Assert
        result.Should().NotBeNull();
        result.Limits.Should().NotBeEmpty();
        
        var deserializedLimits = JsonSerializer.Deserialize<List<object>>(result.Limits, JsonOptions);
        deserializedLimits.Should().HaveCount(3); // 1 initial + 2 additional limits
    }

    #endregion

    #region ToDomain Tests

    [Fact]
    public void ToDomain_WhenModelIsValid_ShouldMapCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        var accountModel = new
        {
            number = "1-123456",
            createdAt = DateTime.UtcNow.AddDays(-5)
        };

        var limitEntryModel = new[]
        {
            new { amount = 1000.00m, description = "Daily Withdrawal Limit" }
        };

        var model = new CustomerDynamoDbModel
        {
            PK = CustomerDynamoDbModel.CreatePartitionKey(customerId),
            SK = "CUSTOMER",
            CustomerId = customerId.ToString(),
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Account = JsonSerializer.Serialize(accountModel, JsonOptions),
            Limits = JsonSerializer.Serialize(limitEntryModel, JsonOptions),
            CreatedAt = createdAt.ToString("O"),
            UpdatedAt = updatedAt.ToString("O"),
            Version = 3,
            IsActive = true,
            GSI1PK = CustomerDynamoDbModel.CreateGsi1PartitionKey("11144477735"),
            GSI1SK = "CUSTOMER"
        };

        // Act
        var result = CustomerMapper.ToDomain(model);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.DocumentNumber.Value.Should().Be("11144477735");
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john.doe@email.com");
        result.Account.Should().NotBeNull();
        result.Account!.Number.Should().Be("0001-00123456");
        result.LimitEntries.Should().HaveCount(1);
        result.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromHours(24));
        result.UpdatedAt.Should().BeCloseTo(updatedAt, TimeSpan.FromHours(24));
        result.Version.Should().Be(3);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToDomain_WhenModelIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => CustomerMapper.ToDomain(null!));
    }

    [Fact]
    public void ToDomain_WhenDocumentNumberIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "invalid_document_number",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(new { number = "1-123456", createdAt = DateTime.UtcNow }, JsonOptions),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid document number");
    }

    [Fact]
    public void ToDomain_WhenCustomerIdIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = "invalid_guid",
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(new { number = "1-123456", createdAt = DateTime.UtcNow }, JsonOptions),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid customer ID");
    }

    [Fact]
    public void ToDomain_WhenCreatedAtIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(new { number = "1-123456", createdAt = DateTime.UtcNow }, JsonOptions),
            CreatedAt = "invalid_date",
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid created date");
    }

    [Fact]
    public void ToDomain_WhenUpdatedAtIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(new { number = "1-123456", createdAt = DateTime.UtcNow }, JsonOptions),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = "invalid_date"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid updated date");
    }

    [Fact]
    public void ToDomain_WhenAccountIsEmptyOrNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = string.Empty,
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Account data is required");
    }

    [Fact]
    public void ToDomain_WhenAccountJsonIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = "invalid_json",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid account data");
    }

    [Fact]
    public void ToDomain_WhenAccountNumberFormatIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidAccountModel = new { number = "invalid_format", createdAt = DateTime.UtcNow };
        
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(invalidAccountModel, JsonOptions),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid account number format");
    }

    [Fact]
    public void ToDomain_WhenLimitsJsonIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var accountModel = new { number = "1-123456", createdAt = DateTime.UtcNow };
        
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(accountModel, JsonOptions),
            Limits = "invalid_limits_json",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CustomerMapper.ToDomain(model));
        exception.Message.Should().Contain("Invalid limit entries data");
    }

    [Fact]
    public void ToDomain_WhenLimitsIsEmpty_ShouldCreateCustomerWithEmptyLimits()
    {
        // Arrange
        var accountModel = new { number = "1-123456", createdAt = DateTime.UtcNow };
        
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(accountModel, JsonOptions),
            Limits = string.Empty,
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = CustomerMapper.ToDomain(model);

        // Assert
        result.Should().NotBeNull();
        result.LimitEntries.Should().BeEmpty();
    }

    [Fact]
    public void ToDomain_WhenModelHasValidMultipleLimits_ShouldMapAllLimits()
    {
        // Arrange
        var accountModel = new { number = "1-123456", createdAt = DateTime.UtcNow };
        var limitEntries = new[]
        {
            new { amount = 1000.00m, description = "Daily Limit" },
            new { amount = 5000.00m, description = "Monthly Limit" }
        };
        
        var model = new CustomerDynamoDbModel
        {
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "Test Customer",
            Email = "test@email.com",
            Account = JsonSerializer.Serialize(accountModel, JsonOptions),
            Limits = JsonSerializer.Serialize(limitEntries, JsonOptions),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O")
        };

        // Act
        var result = CustomerMapper.ToDomain(model);

        // Assert
        result.Should().NotBeNull();
        result.LimitEntries.Should().HaveCount(2);
        result.LimitEntries.Should().Contain(l => l.Amount == 1000.00m && l.Description == "Daily Limit");
        result.LimitEntries.Should().Contain(l => l.Amount == 5000.00m && l.Description == "Monthly Limit");
    }

    #endregion

    #region Round Trip Tests

    [Fact]
    public void ToModel_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalCustomer = InfrastructureTestBuilders.ACustomer()
            .WithId(Guid.NewGuid())
            .WithDocumentNumber("52998224725")
            .WithName("Round Trip Customer")
            .WithEmail("roundtrip@email.com")
            .WithAccount(InfrastructureTestBuilders.AnAccount()
                .WithNumber("987654")
                .WithAgency(Agency.Agency2)
                .Build())
            .WithLimitEntries(
                InfrastructureTestBuilders.ALimitEntry()
                    .WithAmount(2000.00m)
                    .WithDescription("Daily Limit")
                    .Build(),
                InfrastructureTestBuilders.ALimitEntry()
                    .WithAmount(10000.00m)
                    .WithDescription("Monthly Limit")
                    .Build())
            .WithVersion(5)
            .WithIsActive(false)
            .Build();

        // Act
        var model = CustomerMapper.ToModel(originalCustomer);
        var reconstructedCustomer = CustomerMapper.ToDomain(model);

        // Assert
        reconstructedCustomer.Should().NotBeNull();
        reconstructedCustomer.Id.Should().Be(originalCustomer.Id);
        reconstructedCustomer.DocumentNumber.Value.Should().Be(originalCustomer.DocumentNumber.Value);
        reconstructedCustomer.Name.Should().Be(originalCustomer.Name);
        reconstructedCustomer.Email.Should().Be(originalCustomer.Email);
        reconstructedCustomer.Version.Should().Be(originalCustomer.Version);
        reconstructedCustomer.IsActive.Should().Be(originalCustomer.IsActive);
        
        // Account comparison
        reconstructedCustomer.Account.Should().NotBeNull();
        reconstructedCustomer.Account!.Number.Should().Be(originalCustomer.Account!.Number);
        
        // Note: Due to the nature of the mapping, some properties might not be exactly preserved
        // The test verifies that the essential data is maintained through the round trip
    }

    #endregion
}
