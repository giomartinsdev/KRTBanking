using FluentAssertions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.ValueObjects;

/// <summary>
/// Tests for Agency enum using AAA pattern.
/// Tests cover agency enum values and behavior.
/// </summary>
public class AgencyTests
{
    #region Enum Values Tests

    [Fact]
    public void Agency_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        Agency.Agency1.Should().Be((Agency)1);
        Agency.Agency2.Should().Be((Agency)2);
        Agency.Agency3.Should().Be((Agency)3);
    }

    [Fact]
    public void Agency_ShouldHaveCorrectUnderlyingValues()
    {
        // Arrange & Act & Assert
        ((int)Agency.Agency1).Should().Be(1);
        ((int)Agency.Agency2).Should().Be(2);
        ((int)Agency.Agency3).Should().Be(3);
    }

    #endregion

    #region Conversion Tests

    [Theory]
    [InlineData(1, Agency.Agency1)]
    [InlineData(2, Agency.Agency2)]
    [InlineData(3, Agency.Agency3)]
    public void CastFromInt_WhenValidValue_ShouldReturnCorrectAgency(int value, Agency expectedAgency)
    {
        // Arrange & Act
        var result = (Agency)value;

        // Assert
        result.Should().Be(expectedAgency);
    }

    [Theory]
    [InlineData(Agency.Agency1, 1)]
    [InlineData(Agency.Agency2, 2)]
    [InlineData(Agency.Agency3, 3)]
    public void CastToInt_WhenValidAgency_ShouldReturnCorrectValue(Agency agency, int expectedValue)
    {
        // Arrange & Act
        var result = (int)agency;

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region ToString Tests

    [Theory]
    [InlineData(Agency.Agency1, "Agency1")]
    [InlineData(Agency.Agency2, "Agency2")]
    [InlineData(Agency.Agency3, "Agency3")]
    public void ToString_WhenValidAgency_ShouldReturnCorrectString(Agency agency, string expectedString)
    {
        // Arrange & Act
        var result = agency.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WhenSameAgency_ShouldReturnTrue()
    {
        // Arrange
        var agency1 = Agency.Agency1;
        var agency2 = Agency.Agency1;

        // Act & Assert
        agency1.Equals(agency2).Should().BeTrue();
        (agency1 == agency2).Should().BeTrue();
        (agency1 != agency2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenDifferentAgency_ShouldReturnFalse()
    {
        // Arrange
        var agency1 = Agency.Agency1;
        var agency2 = Agency.Agency2;

        // Act & Assert
        agency1.Equals(agency2).Should().BeFalse();
        (agency1 == agency2).Should().BeFalse();
        (agency1 != agency2).Should().BeTrue();
    }

    #endregion

    #region IsDefined Tests

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(0, false)]
    [InlineData(4, false)]
    [InlineData(-1, false)]
    public void IsDefined_WhenCheckingValues_ShouldReturnExpectedResult(int value, bool expectedResult)
    {
        // Arrange & Act
        var result = Enum.IsDefined(typeof(Agency), value);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}
