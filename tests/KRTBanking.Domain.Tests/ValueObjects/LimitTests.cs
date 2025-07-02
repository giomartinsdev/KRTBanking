using FluentAssertions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.ValueObjects;

public class LimitTests
{
    [Fact]
    public void Constructor_ValidAmountAndDescription_ShouldCreateLimit()
    {
        // Arrange
        var amount = 5000m;
        var description = "Credit Limit";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var limit = new Limit(amount, description);
        var afterCreation = DateTime.UtcNow;

        // Assert
        limit.Amount.Should().Be(amount);
        limit.Description.Should().Be(description);
        limit.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        limit.CreatedAt.Should().BeOnOrBefore(afterCreation);
        limit.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Constructor_ValidAmountOnly_ShouldCreateLimitWithDefaultDescription()
    {
        // Arrange
        var amount = 3000m;

        // Act
        var limit = new Limit(amount);

        // Assert
        limit.Amount.Should().Be(amount);
        limit.Description.Should().Be("Credit Limit");
    }

    [Fact]
    public void Constructor_NegativeAmount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var amount = -1000m;
        var description = "Invalid Limit";

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Limit(amount, description));
        exception.ParamName.Should().Be("amount");
        exception.Message.Should().Contain("Limit amount cannot be negative");
    }

    [Fact]
    public void Constructor_ZeroAmount_ShouldCreateLimit()
    {
        // Arrange
        var amount = 0m;
        var description = "Zero Limit";

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Amount.Should().Be(0m);
        limit.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_InvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Arrange
        var amount = 1000m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Limit(amount, invalidDescription));
        exception.ParamName.Should().Be("description");
        exception.Message.Should().Contain("Limit description cannot be null or empty");
    }

    [Fact]
    public void Constructor_DescriptionWithWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var amount = 2000m;
        var description = "  Credit Limit  ";

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Description.Should().Be("Credit Limit");
    }

    [Fact]
    public void Create_ValidParameters_ShouldCreateLimit()
    {
        // Arrange
        var amount = 7500m;
        var description = "Business Credit";

        // Act
        var limit = Limit.Create(amount, description);

        // Assert
        limit.Amount.Should().Be(amount);
        limit.Description.Should().Be(description);
    }

    [Fact]
    public void Equals_SameLimits_ShouldReturnTrue()
    {
        // Arrange
        var limit1 = new Limit(5000m, "Credit Limit");
        var limit2 = new Limit(5000m, "Credit Limit");

        // Act & Assert
        limit1.Equals(limit2).Should().BeTrue();
        limit1.Equals((object)limit2).Should().BeTrue();
        (limit1 == limit2).Should().BeTrue();
        (limit1 != limit2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentAmounts_ShouldReturnFalse()
    {
        // Arrange
        var limit1 = new Limit(5000m, "Credit Limit");
        var limit2 = new Limit(3000m, "Credit Limit");

        // Act & Assert
        limit1.Equals(limit2).Should().BeFalse();
        limit1.Equals((object)limit2).Should().BeFalse();
        (limit1 == limit2).Should().BeFalse();
        (limit1 != limit2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentDescriptions_ShouldReturnFalse()
    {
        // Arrange
        var limit1 = new Limit(5000m, "Credit Limit");
        var limit2 = new Limit(5000m, "Business Limit");

        // Act & Assert
        limit1.Equals(limit2).Should().BeFalse();
        limit1.Equals((object)limit2).Should().BeFalse();
        (limit1 == limit2).Should().BeFalse();
        (limit1 != limit2).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullLimit_ShouldReturnFalse()
    {
        // Arrange
        var limit = new Limit(5000m, "Credit Limit");

        // Act & Assert
        limit.Equals(null).Should().BeFalse();
        limit.Equals((object?)null).Should().BeFalse();
        (limit == null).Should().BeFalse();
        (null == limit).Should().BeFalse();
        (limit != null).Should().BeTrue();
        (null != limit).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var limit = new Limit(5000m, "Credit Limit");
        var differentObject = "not a limit";

        // Act & Assert
        limit.Equals(differentObject).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameLimits_ShouldReturnSameHashCode()
    {
        // Arrange
        var limit1 = new Limit(5000m, "Credit Limit");
        var limit2 = new Limit(5000m, "Credit Limit");

        // Act & Assert
        limit1.GetHashCode().Should().Be(limit2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentLimits_ShouldReturnDifferentHashCodes()
    {
        // Arrange
        var limit1 = new Limit(5000m, "Credit Limit");
        var limit2 = new Limit(3000m, "Credit Limit");

        // Act & Assert
        limit1.GetHashCode().Should().NotBe(limit2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var limit = new Limit(5000m, "Credit Limit");

        // Act
        var result = limit.ToString();

        // Assert
        result.Should().Contain("Credit Limit");
        result.Should().StartWith("Limit:");
    }

    [Fact]
    public void Constructor_LargeAmount_ShouldCreateLimit()
    {
        // Arrange
        var amount = decimal.MaxValue;
        var description = "Maximum Limit";

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Amount.Should().Be(amount);
        limit.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_VeryPreciseAmount_ShouldCreateLimit()
    {
        // Arrange
        var amount = 1234.5678m;
        var description = "Precise Limit";

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Amount.Should().Be(amount);
        limit.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_SpecialCharactersInDescription_ShouldCreateLimit()
    {
        // Arrange
        var amount = 1000m;
        var description = "Credit Limit (Special-Characters_123)";

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_LongDescription_ShouldCreateLimit()
    {
        // Arrange
        var amount = 1000m;
        var description = new string('A', 1000);

        // Act
        var limit = new Limit(amount, description);

        // Assert
        limit.Description.Should().Be(description);
        limit.Description.Should().HaveLength(1000);
    }

    [Fact]
    public void CreatedAt_ShouldNotBeEqualForDifferentInstances()
    {
        // Arrange & Act
        var limit1 = new Limit(1000m, "Limit 1");
        Task.Delay(1).GetAwaiter();
        var limit2 = new Limit(1000m, "Limit 1");

        // Assert
        limit1.CreatedAt.Should().NotBe(limit2.CreatedAt);
    }
}
