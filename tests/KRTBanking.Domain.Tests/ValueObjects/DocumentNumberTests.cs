using FluentAssertions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.ValueObjects;

/// <summary>
/// Tests for DocumentNumber value object using AAA pattern.
/// Tests cover CPF validation, formatting, and equality behavior.
/// </summary>
public class DocumentNumberTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WhenValidCpf_ShouldCreateInstance()
    {
        // Arrange
        var validCpf = "11144477735"; // Valid CPF

        // Act
        var result = new DocumentNumber(validCpf);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(validCpf);
    }

    [Fact]
    public void Constructor_WhenValidCpfWithFormatting_ShouldRemoveFormattingAndCreateInstance()
    {
        // Arrange
        var formattedCpf = "111.444.777-35";
        var expectedCleanCpf = "11144477735";

        // Act
        var result = new DocumentNumber(formattedCpf);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedCleanCpf);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenNullOrWhitespace_ShouldThrowArgumentException(string invalidValue)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DocumentNumber(invalidValue));
        exception.Message.Should().Contain("Document number cannot be null or empty");
        exception.ParamName.Should().Be("value");
    }

    [Theory]
    [InlineData("1234567890")] // 10 digits
    [InlineData("123456789012")] // 12 digits
    [InlineData("abcdefghijk")] // letters
    [InlineData("123abc78901")] // mixed
    public void Constructor_WhenInvalidFormat_ShouldThrowArgumentException(string invalidFormat)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DocumentNumber(invalidFormat));
        exception.Message.Should().Contain("Invalid document number format");
        exception.ParamName.Should().Be("value");
    }

    [Theory]
    [InlineData("11111111111")] // All same digits
    [InlineData("12345678901")] // Invalid CPF checksum
    [InlineData("00000000000")] // All zeros
    public void Constructor_WhenInvalidCpf_ShouldThrowArgumentException(string invalidCpf)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DocumentNumber(invalidCpf));
        exception.Message.Should().Contain("Invalid CPF document number");
        exception.ParamName.Should().Be("value");
    }

    #endregion

    #region TryCreate Tests

    [Fact]
    public void TryCreate_WhenValidCpf_ShouldReturnDocumentNumber()
    {
        // Arrange
        var validCpf = "52998224725"; // Valid CPF

        // Act
        var result = DocumentNumber.TryCreate(validCpf);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(validCpf);
    }

    [Fact]
    public void TryCreate_WhenInvalidCpf_ShouldReturnNull()
    {
        // Arrange
        var invalidCpf = "12345678901";

        // Act
        var result = DocumentNumber.TryCreate(invalidCpf);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryCreate_WhenNullValue_ShouldReturnNull()
    {
        // Arrange & Act
        var result = DocumentNumber.TryCreate(null!);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WhenSameValue_ShouldReturnTrue()
    {
        // Arrange
        var cpf = "11144477735";
        var documentNumber1 = new DocumentNumber(cpf);
        var documentNumber2 = new DocumentNumber(cpf);

        // Act & Assert
        documentNumber1.Equals(documentNumber2).Should().BeTrue();
        (documentNumber1 == documentNumber2).Should().BeTrue();
        (documentNumber1 != documentNumber2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var documentNumber1 = new DocumentNumber("11144477735");
        var documentNumber2 = new DocumentNumber("52998224725");

        // Act & Assert
        documentNumber1.Equals(documentNumber2).Should().BeFalse();
        (documentNumber1 == documentNumber2).Should().BeFalse();
        (documentNumber1 != documentNumber2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenComparedToNull_ShouldReturnFalse()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");

        // Act & Assert
        documentNumber.Equals(null).Should().BeFalse();
        (documentNumber == null).Should().BeFalse();
        (documentNumber != null).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WhenSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var cpf = "11144477735";
        var documentNumber1 = new DocumentNumber(cpf);
        var documentNumber2 = new DocumentNumber(cpf);

        // Act & Assert
        documentNumber1.GetHashCode().Should().Be(documentNumber2.GetHashCode());
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void ImplicitStringConversion_WhenValidDocumentNumber_ShouldReturnValue()
    {
        // Arrange
        var cpf = "11144477735";
        var documentNumber = new DocumentNumber(cpf);

        // Act
        string result = documentNumber;

        // Assert
        result.Should().Be(cpf);
    }

    [Fact]
    public void ImplicitStringConversion_WhenNullDocumentNumber_ShouldReturnEmptyString()
    {
        // Arrange
        DocumentNumber? documentNumber = null;

        // Act
        string result = documentNumber!;

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ToString_WhenValidDocumentNumber_ShouldReturnFormattedValue()
    {
        // Arrange
        var cpf = "11144477735";
        var documentNumber = new DocumentNumber(cpf);

        // Act
        var result = documentNumber.ToString();

        // Assert
        result.Should().Be("111.444.777-35");
    }

    #endregion
}
