using FluentAssertions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.ValueObjects;

/// <summary>
/// Tests for LimitEntry value object using AAA pattern.
/// Tests cover limit entry creation, validation, and behavior.
/// </summary>
public class LimitEntryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WhenValidAmountAndDescription_ShouldCreateInstance()
    {
        // Arrange
        var amount = 1000.50m;
        var description = "Daily withdrawal limit";

        // Act
        var result = new LimitEntry(amount, description);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(amount);
        result.Description.Should().Be(description);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldCreateLimitEntry()
    {
        // Arrange
        var amount = -500m;
        var description = "Credit decrease";

        // Act
        var limitEntry = new LimitEntry(amount, description);

        // Assert
        limitEntry.Amount.Should().Be(amount);
        limitEntry.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Arrange
        var amount = 1000m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new LimitEntry(amount, invalidDescription));
        exception.Message.Should().Contain("Limit entry description cannot be null or empty");
        exception.ParamName.Should().Be("description");
    }

    [Fact]
    public void Constructor_WhenZeroAmount_ShouldCreateInstance()
    {
        // Arrange
        var amount = 0m;
        var description = "Zero limit";

        // Act
        var result = new LimitEntry(amount, description);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0m);
        result.Description.Should().Be(description);
    }

    #endregion

    #region Equality Tests
    [Fact]
    public void Equals_WhenDifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var description = "Test limit";
        
        var limitEntry1 = new LimitEntry(1000m, description);
        var limitEntry2 = new LimitEntry(2000m, description);

        // Act & Assert
        limitEntry1.Equals(limitEntry2).Should().BeFalse();
        (limitEntry1 == limitEntry2).Should().BeFalse();
        (limitEntry1 != limitEntry2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenDifferentDescription_ShouldReturnFalse()
    {
        // Arrange
        var amount = 1000m;
        
        var limitEntry1 = new LimitEntry(amount, "Description 1");
        var limitEntry2 = new LimitEntry(amount, "Description 2");

        // Act & Assert
        limitEntry1.Equals(limitEntry2).Should().BeFalse();
        (limitEntry1 == limitEntry2).Should().BeFalse();
        (limitEntry1 != limitEntry2).Should().BeTrue();
    }
    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WhenValidLimitEntry_ShouldReturnFormattedString()
    {
        // Arrange
        var amount = 1000.50m;
        var description = "Test limit";
        var limitEntry = new LimitEntry(amount, description);

        // Act
        var result = limitEntry.ToString();

        // Assert
        result.Should().Contain(amount.ToString("C"));
        result.Should().Contain(description);
    }

    #endregion
}
