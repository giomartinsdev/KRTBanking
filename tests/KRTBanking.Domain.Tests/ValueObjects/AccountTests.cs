using FluentAssertions;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.ValueObjects;

/// <summary>
/// Tests for Account value object using AAA pattern.
/// Tests cover account creation, formatting, and validation.
/// </summary>
public class AccountTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WhenValidAgencyAndNumber_ShouldCreateInstance()
    {
        // Arrange
        var agency = Agency.Agency1;
        var accountNumber = 123456;

        // Act
        var result = new Account(agency, accountNumber);

        // Assert
        result.Should().NotBeNull();
        result.Agency.Should().Be(agency);
        result.AccountNumber.Should().Be(accountNumber);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Constructor_WhenInvalidAccountNumber_ShouldThrowArgumentOutOfRangeException(int invalidAccountNumber)
    {
        // Arrange
        var agency = Agency.Agency1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Account(agency, invalidAccountNumber));
        exception.Message.Should().Contain("Account number must be positive");
        exception.ParamName.Should().Be("accountNumber");
    }

    #endregion

    #region Number Property Tests

    [Theory]
    [InlineData(Agency.Agency1, 123456, "0001-00123456")]
    [InlineData(Agency.Agency2, 1, "0002-00000001")]
    [InlineData(Agency.Agency3, 999999, "0003-00999999")]
    public void Number_WhenValidAccount_ShouldReturnFormattedNumber(Agency agency, int accountNumber, string expectedFormat)
    {
        // Arrange
        var account = new Account(agency, accountNumber);

        // Act
        var result = account.Number;

        // Assert
        result.Should().Be(expectedFormat);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WhenSameAgencyAndAccountNumber_ShouldReturnTrue()
    {
        // Arrange
        var agency = Agency.Agency1;
        var accountNumber = 123456;
        var account1 = new Account(agency, accountNumber);
        var account2 = new Account(agency, accountNumber);

        // Act & Assert
        account1.Equals(account2).Should().BeTrue();
        (account1 == account2).Should().BeTrue();
        (account1 != account2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenDifferentAgency_ShouldReturnFalse()
    {
        // Arrange
        var accountNumber = 123456;
        var account1 = new Account(Agency.Agency1, accountNumber);
        var account2 = new Account(Agency.Agency2, accountNumber);

        // Act & Assert
        account1.Equals(account2).Should().BeFalse();
        (account1 == account2).Should().BeFalse();
        (account1 != account2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenDifferentAccountNumber_ShouldReturnFalse()
    {
        // Arrange
        var agency = Agency.Agency1;
        var account1 = new Account(agency, 123456);
        var account2 = new Account(agency, 654321);

        // Act & Assert
        account1.Equals(account2).Should().BeFalse();
        (account1 == account2).Should().BeFalse();
        (account1 != account2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenComparedToNull_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account(Agency.Agency1, 123456);

        // Act & Assert
        account.Equals(null).Should().BeFalse();
        (account == null).Should().BeFalse();
        (account != null).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WhenSameAgencyAndAccountNumber_ShouldReturnSameHashCode()
    {
        // Arrange
        var agency = Agency.Agency1;
        var accountNumber = 123456;
        var account1 = new Account(agency, accountNumber);
        var account2 = new Account(agency, accountNumber);

        // Act & Assert
        account1.GetHashCode().Should().Be(account2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WhenValidAccount_ShouldReturnFormattedString()
    {
        // Arrange
        var account = new Account(Agency.Agency1, 123456);

        // Act
        var result = account.ToString();

        // Assert
        result.Should().Be("0001-00123456");
    }

    #endregion
}
