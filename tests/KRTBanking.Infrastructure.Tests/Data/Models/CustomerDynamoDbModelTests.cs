using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using KRTBanking.Infrastructure.Data.Models;
using KRTBanking.Infrastructure.Tests.Builders;
using Xunit;

namespace KRTBanking.Infrastructure.Tests.Data.Models;

/// <summary>
/// Tests for CustomerDynamoDbModel using AAA pattern.
/// Tests cover serialization, deserialization, and key generation methods.
/// </summary>
public class CustomerDynamoDbModelTests
{
    #region ToDynamoDbItem Tests

    [Fact]
    public void ToDynamoDbItem_WhenModelIsValid_ShouldCreateCorrectAttributeValues()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var model = new CustomerDynamoDbModel
        {
            PK = CustomerDynamoDbModel.CreatePartitionKey(customerId),
            SK = "CUSTOMER",
            CustomerId = customerId.ToString(),
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Account = """{"number": "1-123456", "createdAt": "2024-01-01T00:00:00Z"}""",
            Limits = """[{"type": "Daily", "amount": 1000.00}]""",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            Version = 2,
            IsActive = true,
            GSI1PK = CustomerDynamoDbModel.CreateGsi1PartitionKey("11144477735"),
            GSI1SK = "CUSTOMER"
        };

        // Act
        var result = model.ToDynamoDbItem();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("PK");
        result["PK"].S.Should().Be(model.PK);
        
        result.Should().ContainKey("SK");
        result["SK"].S.Should().Be("CUSTOMER");
        
        result.Should().ContainKey("CustomerId");
        result["CustomerId"].S.Should().Be(customerId.ToString());
        
        result.Should().ContainKey("DocumentNumber");
        result["DocumentNumber"].S.Should().Be("11144477735");
        
        result.Should().ContainKey("Name");
        result["Name"].S.Should().Be("John Doe");
        
        result.Should().ContainKey("Email");
        result["Email"].S.Should().Be("john.doe@email.com");
        
        result.Should().ContainKey("Account");
        result["Account"].S.Should().Be(model.Account);
        
        result.Should().ContainKey("Limits");
        result["Limits"].S.Should().Be(model.Limits);
        
        result.Should().ContainKey("CreatedAt");
        result["CreatedAt"].S.Should().Be(model.CreatedAt);
        
        result.Should().ContainKey("UpdatedAt");
        result["UpdatedAt"].S.Should().Be(model.UpdatedAt);
        
        result.Should().ContainKey("Version");
        result["Version"].N.Should().Be("2");
        
        result.Should().ContainKey("IsActive");
        result["IsActive"].BOOL.Should().BeTrue();
        
        result.Should().ContainKey("GSI1PK");
        result["GSI1PK"].S.Should().Be(model.GSI1PK);
        
        result.Should().ContainKey("GSI1SK");
        result["GSI1SK"].S.Should().Be("CUSTOMER");
    }

    [Fact]
    public void ToDynamoDbItem_WhenAccountIsNull_ShouldNotIncludeAccountAttribute()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            PK = "CUSTOMER#123",
            SK = "CUSTOMER",
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Account = null,
            Limits = "[]",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            Version = 1,
            IsActive = true,
            GSI1PK = "DOC#11144477735",
            GSI1SK = "CUSTOMER"
        };

        // Act
        var result = model.ToDynamoDbItem();

        // Assert
        result.Should().NotContainKey("Account");
    }

    [Fact]
    public void ToDynamoDbItem_WhenAccountIsEmpty_ShouldNotIncludeAccountAttribute()
    {
        // Arrange
        var model = new CustomerDynamoDbModel
        {
            PK = "CUSTOMER#123",
            SK = "CUSTOMER",
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Account = string.Empty,
            Limits = "[]",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            Version = 1,
            IsActive = true,
            GSI1PK = "DOC#11144477735",
            GSI1SK = "CUSTOMER"
        };

        // Act
        var result = model.ToDynamoDbItem();

        // Assert
        result.Should().NotContainKey("Account");
    }

    [Fact]
    public void ToDynamoDbItem_WhenAccountHasValue_ShouldIncludeAccountAttribute()
    {
        // Arrange
        var accountJson = """{"number": "1-123456"}""";
        var model = new CustomerDynamoDbModel
        {
            PK = "CUSTOMER#123",
            SK = "CUSTOMER",
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Account = accountJson,
            Limits = "[]",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            Version = 1,
            IsActive = true,
            GSI1PK = "DOC#11144477735",
            GSI1SK = "CUSTOMER"
        };

        // Act
        var result = model.ToDynamoDbItem();

        // Assert
        result.Should().ContainKey("Account");
        result["Account"].S.Should().Be(accountJson);
    }

    #endregion

    #region FromDynamoDbItem Tests

    [Fact]
    public void FromDynamoDbItem_WhenItemIsComplete_ShouldCreateModelCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var item = InfrastructureTestBuilders.ADynamoDbItem()
            .WithCustomerAttributes(customerId, "11144477735", "Jane Doe", "jane.doe@email.com")
            .WithAttribute("Account", """{"number": "1-987654"}""")
            .WithAttribute("Limits", """[{"type": "Monthly", "amount": 5000.00}]""")
            .WithAttribute("Version", 3L)
            .WithAttribute("IsActive", false)
            .Build();

        // Act
        var result = CustomerDynamoDbModel.FromDynamoDbItem(item);

        // Assert
        result.Should().NotBeNull();
        result.PK.Should().Be(item["PK"].S);
        result.SK.Should().Be("CUSTOMER");
        result.CustomerId.Should().Be(customerId.ToString());
        result.DocumentNumber.Should().Be("11144477735");
        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane.doe@email.com");
        result.Account.Should().Be("""{"number": "1-987654"}""");
        result.Limits.Should().Be("""[{"type": "Monthly", "amount": 5000.00}]""");
        result.Version.Should().Be(3);
        result.IsActive.Should().BeFalse();
        result.GSI1PK.Should().Be(item["GSI1PK"].S);
        result.GSI1SK.Should().Be("CUSTOMER");
    }

    [Fact]
    public void FromDynamoDbItem_WhenItemIsMissingOptionalFields_ShouldUseDefaults()
    {
        // Arrange
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "CUSTOMER#123" },
            ["SK"] = new AttributeValue { S = "CUSTOMER" },
            ["CustomerId"] = new AttributeValue { S = Guid.NewGuid().ToString() },
            ["DocumentNumber"] = new AttributeValue { S = "11144477735" }
            // Missing: Name, Email, Account, Limits, CreatedAt, UpdatedAt, Version, IsActive, GSI1PK, GSI1SK
        };

        // Act
        var result = CustomerDynamoDbModel.FromDynamoDbItem(item);

        // Assert
        result.Should().NotBeNull();
        result.PK.Should().Be("CUSTOMER#123");
        result.SK.Should().Be("CUSTOMER");
        result.DocumentNumber.Should().Be("11144477735");
        result.Name.Should().BeEmpty();
        result.Email.Should().BeEmpty();
        result.Account.Should().BeNull();
        result.Limits.Should().Be("[]");
        result.CreatedAt.Should().BeEmpty();
        result.UpdatedAt.Should().BeEmpty();
        result.Version.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.GSI1PK.Should().BeEmpty();
        result.GSI1SK.Should().BeEmpty();
    }

    [Fact]
    public void FromDynamoDbItem_WhenVersionIsInvalid_ShouldDefaultToOne()
    {
        // Arrange
        var item = InfrastructureTestBuilders.ADynamoDbItem()
            .WithCustomerAttributes(Guid.NewGuid(), "11144477735", "Test", "test@email.com")
            .WithAttribute("Version", "invalid_number")
            .Build();

        // Act
        var result = CustomerDynamoDbModel.FromDynamoDbItem(item);

        // Assert
        result.Version.Should().Be(1);
    }

    [Fact]
    public void FromDynamoDbItem_WhenIsActiveIsMissing_ShouldDefaultToTrue()
    {
        // Arrange
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "CUSTOMER#123" },
            ["SK"] = new AttributeValue { S = "CUSTOMER" },
            ["CustomerId"] = new AttributeValue { S = Guid.NewGuid().ToString() },
            ["DocumentNumber"] = new AttributeValue { S = "11144477735" }
            // IsActive is missing
        };

        // Act
        var result = CustomerDynamoDbModel.FromDynamoDbItem(item);

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FromDynamoDbItem_WhenEmptyDictionary_ShouldCreateModelWithDefaults()
    {
        // Arrange
        var emptyItem = new Dictionary<string, AttributeValue>();

        // Act
        var result = CustomerDynamoDbModel.FromDynamoDbItem(emptyItem);

        // Assert
        result.Should().NotBeNull();
        result.PK.Should().BeEmpty();
        result.SK.Should().BeEmpty();
        result.CustomerId.Should().BeEmpty();
        result.DocumentNumber.Should().BeEmpty();
        result.Name.Should().BeEmpty();
        result.Email.Should().BeEmpty();
        result.Account.Should().BeNull();
        result.Limits.Should().Be("[]");
        result.CreatedAt.Should().BeEmpty();
        result.UpdatedAt.Should().BeEmpty();
        result.Version.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.GSI1PK.Should().BeEmpty();
        result.GSI1SK.Should().BeEmpty();
    }

    #endregion

    #region Key Generation Tests

    [Fact]
    public void CreatePartitionKey_WhenCustomerIdIsValid_ShouldCreateCorrectKey()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = CustomerDynamoDbModel.CreatePartitionKey(customerId);

        // Assert
        result.Should().Be($"CUSTOMER#{customerId}");
    }

    [Fact]
    public void CreatePartitionKey_WhenDifferentCustomerIds_ShouldCreateDifferentKeys()
    {
        // Arrange
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        // Act
        var key1 = CustomerDynamoDbModel.CreatePartitionKey(customerId1);
        var key2 = CustomerDynamoDbModel.CreatePartitionKey(customerId2);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().StartWith("CUSTOMER#");
        key2.Should().StartWith("CUSTOMER#");
    }

    [Fact]
    public void CreateGsi1PartitionKey_WhenDocumentNumberIsValid_ShouldCreateCorrectKey()
    {
        // Arrange
        var documentNumber = "11144477735";

        // Act
        var result = CustomerDynamoDbModel.CreateGsi1PartitionKey(documentNumber);

        // Assert
        result.Should().Be($"DOC#{documentNumber}");
    }

    [Fact]
    public void CreateGsi1PartitionKey_WhenDifferentDocumentNumbers_ShouldCreateDifferentKeys()
    {
        // Arrange
        var docNumber1 = "52998224725";
        var docNumber2 = "11144477735";

        // Act
        var key1 = CustomerDynamoDbModel.CreateGsi1PartitionKey(docNumber1);
        var key2 = CustomerDynamoDbModel.CreateGsi1PartitionKey(docNumber2);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().Be("DOC#52998224725");
        key2.Should().Be("DOC#11144477735");
    }

    [Fact]
    public void CreateGsi1PartitionKey_WhenDocumentNumberIsEmpty_ShouldCreateKeyWithEmptyValue()
    {
        // Arrange
        var emptyDocumentNumber = string.Empty;

        // Act
        var result = CustomerDynamoDbModel.CreateGsi1PartitionKey(emptyDocumentNumber);

        // Assert
        result.Should().Be("DOC#");
    }

    [Fact]
    public void CreateGsi1PartitionKey_WhenDocumentNumberIsNull_ShouldCreateKeyWithNullValue()
    {
        // Arrange
        string? nullDocumentNumber = null;

        // Act
        var result = CustomerDynamoDbModel.CreateGsi1PartitionKey(nullDocumentNumber!);

        // Assert
        result.Should().Be("DOC#");
    }

    #endregion

    #region Round Trip Tests

    [Fact]
    public void ToDynamoDbItem_FromDynamoDbItem_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new CustomerDynamoDbModel
        {
            PK = "CUSTOMER#12345678-1234-1234-1234-111444777352",
            SK = "CUSTOMER",
            CustomerId = Guid.NewGuid().ToString(),
            DocumentNumber = "55667788990",
            Name = "Round Trip Test",
            Email = "roundtrip@test.com",
            Account = """{"number": "2001-5555555555", "balance": 2500.75}""",
            Limits = """[{"type": "Daily", "amount": 1500.00}, {"type": "Monthly", "amount": 7500.00}]""",
            CreatedAt = DateTime.UtcNow.AddDays(-30).ToString("O"),
            UpdatedAt = DateTime.UtcNow.AddHours(-2).ToString("O"),
            Version = 7,
            IsActive = false,
            GSI1PK = "DOC#55667788990",
            GSI1SK = "CUSTOMER"
        };

        // Act
        var dynamoDbItem = originalModel.ToDynamoDbItem();
        var reconstructedModel = CustomerDynamoDbModel.FromDynamoDbItem(dynamoDbItem);

        // Assert
        reconstructedModel.Should().NotBeNull();
        reconstructedModel.PK.Should().Be(originalModel.PK);
        reconstructedModel.SK.Should().Be(originalModel.SK);
        reconstructedModel.CustomerId.Should().Be(originalModel.CustomerId);
        reconstructedModel.DocumentNumber.Should().Be(originalModel.DocumentNumber);
        reconstructedModel.Name.Should().Be(originalModel.Name);
        reconstructedModel.Email.Should().Be(originalModel.Email);
        reconstructedModel.Account.Should().Be(originalModel.Account);
        reconstructedModel.Limits.Should().Be(originalModel.Limits);
        reconstructedModel.CreatedAt.Should().Be(originalModel.CreatedAt);
        reconstructedModel.UpdatedAt.Should().Be(originalModel.UpdatedAt);
        reconstructedModel.Version.Should().Be(originalModel.Version);
        reconstructedModel.IsActive.Should().Be(originalModel.IsActive);
        reconstructedModel.GSI1PK.Should().Be(originalModel.GSI1PK);
        reconstructedModel.GSI1SK.Should().Be(originalModel.GSI1SK);
    }

    #endregion
}
