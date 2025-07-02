using FluentAssertions;
using KRTBanking.Application.DTOs.Transaction;
using System.ComponentModel.DataAnnotations;

namespace KRTBanking.Application.Tests.DTOs.Transaction;

public class ExecuteTransactionDtoTests
{
    [Fact]
    public void ExecuteTransactionDto_ValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 100.50m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
        dto.MerchantDocument.Should().Be("12345678901");
        dto.Value.Should().Be(100.50m);
    }

    [Fact]
    public void ExecuteTransactionDto_EmptyMerchantDocument_ShouldFailValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "",
            Value = 100m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().Contain("field is required");
    }

    [Fact]
    public void ExecuteTransactionDto_TooLongMerchantDocument_ShouldFailValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = new string('1', 21), // 21 characters
            Value = 100m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().Contain("maximum length");
    }

    [Fact]
    public void ExecuteTransactionDto_ZeroValue_ShouldFailValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 0m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().Be("Transaction value must be greater than zero");
    }

    [Fact]
    public void ExecuteTransactionDto_NegativeValue_ShouldFailValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = -50m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().Be("Transaction value must be greater than zero");
    }

    [Fact]
    public void ExecuteTransactionDto_MaxValue_ShouldPassValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = decimal.MaxValue
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ExecuteTransactionDto_MinValidLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = "1", // Minimum valid length
            Value = 0.01m // Minimum valid value
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ExecuteTransactionDto_MaxValidLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new ExecuteTransactionDto
        {
            MerchantDocument = new string('1', 20), // Maximum valid length
            Value = 100m
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateDto(ExecuteTransactionDto dto)
    {
        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, true);
        return results;
    }
}

public class TransactionResultDtoTests
{
    [Fact]
    public void TransactionResultDto_Properties_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new TransactionResultDto();

        // Assert
        dto.IsAuthorized.Should().BeFalse();
        dto.Reason.Should().Be(string.Empty);
        dto.RemainingLimit.Should().BeNull();
        dto.TransactionValue.Should().Be(0m);
    }

    [Fact]
    public void TransactionResultDto_AuthorizedTransaction_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var dto = new TransactionResultDto
        {
            IsAuthorized = true,
            Reason = "Transaction approved",
            RemainingLimit = 1500m,
            TransactionValue = 500m
        };

        // Assert
        dto.IsAuthorized.Should().BeTrue();
        dto.Reason.Should().Be("Transaction approved");
        dto.RemainingLimit.Should().Be(1500m);
        dto.TransactionValue.Should().Be(500m);
    }

    [Fact]
    public void TransactionResultDto_DeniedTransaction_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var dto = new TransactionResultDto
        {
            IsAuthorized = false,
            Reason = "Insufficient limit",
            RemainingLimit = null,
            TransactionValue = 2000m
        };

        // Assert
        dto.IsAuthorized.Should().BeFalse();
        dto.Reason.Should().Be("Insufficient limit");
        dto.RemainingLimit.Should().BeNull();
        dto.TransactionValue.Should().Be(2000m);
    }

    [Fact]
    public void TransactionResultDto_ZeroRemainingLimit_ShouldBeValid()
    {
        // Arrange & Act
        var dto = new TransactionResultDto
        {
            IsAuthorized = true,
            Reason = "Transaction approved - limit exhausted",
            RemainingLimit = 0m,
            TransactionValue = 1000m
        };

        // Assert
        dto.IsAuthorized.Should().BeTrue();
        dto.Reason.Should().Be("Transaction approved - limit exhausted");
        dto.RemainingLimit.Should().Be(0m);
        dto.TransactionValue.Should().Be(1000m);
    }

    [Fact]
    public void TransactionResultDto_NegativeRemainingLimit_ShouldBeValid()
    {
        // Arrange & Act
        var dto = new TransactionResultDto
        {
            IsAuthorized = false,
            Reason = "Over limit",
            RemainingLimit = -500m,
            TransactionValue = 1500m
        };

        // Assert
        dto.IsAuthorized.Should().BeFalse();
        dto.Reason.Should().Be("Over limit");
        dto.RemainingLimit.Should().Be(-500m);
        dto.TransactionValue.Should().Be(1500m);
    }

    [Fact]
    public void TransactionResultDto_LargeTransactionValue_ShouldBeValid()
    {
        // Arrange & Act
        var dto = new TransactionResultDto
        {
            IsAuthorized = true,
            Reason = "Large transaction approved",
            RemainingLimit = 5000m,
            TransactionValue = 50000m
        };

        // Assert
        dto.IsAuthorized.Should().BeTrue();
        dto.Reason.Should().Be("Large transaction approved");
        dto.RemainingLimit.Should().Be(5000m);
        dto.TransactionValue.Should().Be(50000m);
    }
}
