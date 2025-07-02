using FluentAssertions;
using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Application.Tests.DTOs.Customer;

public class CustomerDtoTests
{
    [Fact]
    public void CustomerDto_CurrentLimit_ShouldCalculateCorrectSum()
    {
        // Arrange
        var customerDto = new CustomerDto
        {
            LimitEntries = new List<LimitEntryDto>
            {
                new() { Amount = 1000m, Description = "Initial Limit" },
                new() { Amount = 500m, Description = "Increase" },
                new() { Amount = -200m, Description = "Decrease" }
            }
        };

        // Act
        var currentLimit = customerDto.CurrentLimit;

        // Assert
        currentLimit.Should().Be(1300m);
    }

    [Fact]
    public void CustomerDto_CurrentLimit_ShouldReturnZero_WhenNoLimitEntries()
    {
        // Arrange
        var customerDto = new CustomerDto
        {
            LimitEntries = new List<LimitEntryDto>()
        };

        // Act
        var currentLimit = customerDto.CurrentLimit;

        // Assert
        currentLimit.Should().Be(0m);
    }

    [Fact]
    public void CustomerDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var customerDto = new CustomerDto();

        // Assert
        customerDto.Id.Should().Be(Guid.Empty);
        customerDto.DocumentNumber.Should().Be(string.Empty);
        customerDto.Name.Should().Be(string.Empty);
        customerDto.Email.Should().Be(string.Empty);
        customerDto.Account.Should().NotBeNull();
        customerDto.LimitEntries.Should().NotBeNull().And.BeEmpty();
        customerDto.CurrentLimit.Should().Be(0m);
        customerDto.IsActive.Should().BeFalse();
        customerDto.CreatedAt.Should().Be(default);
        customerDto.UpdatedAt.Should().Be(default);
        customerDto.Version.Should().Be(0);
    }

    [Fact]
    public void CustomerDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);
        var account = new AccountDto { Agency = Agency.Agency1, AccountNumber = 12345 };
        var limitEntries = new List<LimitEntryDto>
        {
            new() { Amount = 1000m, Description = "Initial" }
        };

        // Act
        var customerDto = new CustomerDto
        {
            Id = id,
            DocumentNumber = "12345678901",
            Name = "John Doe",
            Email = "john@example.com",
            Account = account,
            LimitEntries = limitEntries,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Version = 5
        };

        // Assert
        customerDto.Id.Should().Be(id);
        customerDto.DocumentNumber.Should().Be("12345678901");
        customerDto.Name.Should().Be("John Doe");
        customerDto.Email.Should().Be("john@example.com");
        customerDto.Account.Should().BeSameAs(account);
        customerDto.LimitEntries.Should().BeSameAs(limitEntries);
        customerDto.IsActive.Should().BeTrue();
        customerDto.CreatedAt.Should().Be(createdAt);
        customerDto.UpdatedAt.Should().Be(updatedAt);
        customerDto.Version.Should().Be(5);
    }
}

public class CreateCustomerDtoTests
{
    [Fact]
    public void CreateCustomerDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CreateCustomerDto();

        // Assert
        dto.DocumentNumber.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.Email.Should().Be(string.Empty);
        dto.Agency.Should().Be(Agency.Agency1);
        dto.AccountNumber.Should().Be(0);
        dto.LimitAmount.Should().Be(0m);
        dto.LimitDescription.Should().Be("Credit Limit");
    }

    [Fact]
    public void CreateCustomerDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateCustomerDto
        {
            DocumentNumber = "12345678901",
            Name = "John Doe",
            Email = "john@example.com",
            Agency = Agency.Agency2,
            AccountNumber = 12345,
            LimitAmount = 5000m,
            LimitDescription = "Business Credit"
        };

        // Assert
        dto.DocumentNumber.Should().Be("12345678901");
        dto.Name.Should().Be("John Doe");
        dto.Email.Should().Be("john@example.com");
        dto.Agency.Should().Be(Agency.Agency2);
        dto.AccountNumber.Should().Be(12345);
        dto.LimitAmount.Should().Be(5000m);
        dto.LimitDescription.Should().Be("Business Credit");
    }
}

public class AccountDtoTests
{
    [Fact]
    public void AccountDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new AccountDto();

        // Assert
        dto.Agency.Should().Be(Agency.Agency1);
        dto.AccountNumber.Should().Be(0);
        dto.Number.Should().Be(string.Empty);
        dto.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void AccountDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new AccountDto
        {
            Agency = Agency.Agency3,
            AccountNumber = 98765,
            Number = "98765-4",
            CreatedAt = createdAt
        };

        // Assert
        dto.Agency.Should().Be(Agency.Agency3);
        dto.AccountNumber.Should().Be(98765);
        dto.Number.Should().Be("98765-4");
        dto.CreatedAt.Should().Be(createdAt);
    }
}

public class LimitEntryDtoTests
{
    [Fact]
    public void LimitEntryDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new LimitEntryDto();

        // Assert
        dto.Amount.Should().Be(0m);
        dto.Description.Should().Be(string.Empty);
        dto.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void LimitEntryDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new LimitEntryDto
        {
            Amount = 2500m,
            Description = "Credit increase",
            CreatedAt = createdAt
        };

        // Assert
        dto.Amount.Should().Be(2500m);
        dto.Description.Should().Be("Credit increase");
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void LimitEntryDto_Amount_ShouldAllowNegativeValues()
    {
        // Arrange & Act
        var dto = new LimitEntryDto
        {
            Amount = -1000m,
            Description = "Credit decrease"
        };

        // Assert
        dto.Amount.Should().Be(-1000m);
        dto.Description.Should().Be("Credit decrease");
    }
}

public class AdjustLimitDtoTests
{
    [Fact]
    public void AdjustLimitDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new AdjustLimitDto();

        // Assert
        dto.Amount.Should().Be(0m);
        dto.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void AdjustLimitDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        var dto = new AdjustLimitDto
        {
            Amount = 1500m,
            Description = "Monthly limit increase"
        };

        // Assert
        dto.Amount.Should().Be(1500m);
        dto.Description.Should().Be("Monthly limit increase");
    }

    [Fact]
    public void AdjustLimitDto_Amount_ShouldAllowNegativeValues()
    {
        // Arrange & Act
        var dto = new AdjustLimitDto
        {
            Amount = -500m,
            Description = "Risk adjustment"
        };

        // Assert
        dto.Amount.Should().Be(-500m);
        dto.Description.Should().Be("Risk adjustment");
    }
}

public class UpdateLimitDtoTests
{
    [Fact]
    public void UpdateLimitDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new UpdateLimitDto();

        // Assert
        dto.LimitAmount.Should().Be(0m);
        dto.LimitDescription.Should().Be("Credit Limit");
    }

    [Fact]
    public void UpdateLimitDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateLimitDto
        {
            LimitAmount = 3000m,
            LimitDescription = "Updated Credit Limit"
        };

        // Assert
        dto.LimitAmount.Should().Be(3000m);
        dto.LimitDescription.Should().Be("Updated Credit Limit");
    }
}

public class PagedCustomersDtoTests
{
    [Fact]
    public void PagedCustomersDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new PagedCustomersDto();

        // Assert
        dto.Customers.Should().NotBeNull().And.BeEmpty();
        dto.NextPageKey.Should().BeNull();
        dto.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PagedCustomersDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Customer 1" },
            new() { Id = Guid.NewGuid(), Name = "Customer 2" }
        };

        // Act
        var dto = new PagedCustomersDto
        {
            Customers = customers,
            NextPageKey = "next-page-key",
            HasNextPage = true
        };

        // Assert
        dto.Customers.Should().BeSameAs(customers);
        dto.NextPageKey.Should().Be("next-page-key");
        dto.HasNextPage.Should().BeTrue();
    }
}

public class DeactivateCustomerDtoTests
{
    [Fact]
    public void DeactivateCustomerDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new DeactivateCustomerDto();

        // Assert
        dto.Reason.Should().Be(string.Empty);
        dto.DeactivatedBy.Should().BeNull();
    }

    [Fact]
    public void DeactivateCustomerDto_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        var dto = new DeactivateCustomerDto
        {
            Reason = "Fraudulent activity detected",
            DeactivatedBy = "admin@bank.com"
        };

        // Assert
        dto.Reason.Should().Be("Fraudulent activity detected");
        dto.DeactivatedBy.Should().Be("admin@bank.com");
    }
}
