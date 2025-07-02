using FluentAssertions;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.Events;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.Entities;

/// <summary>
/// Tests for Customer entity using AAA pattern.
/// Tests cover customer creation, business operations, and domain events.
/// </summary>
public class CustomerTests
{
    #region Create Tests

    [Fact]
    public void Create_WhenValidParameters_ShouldCreateCustomer()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "John Doe";
        var email = "john.doe@email.com";
        var account = new Account(Agency.Agency1, 123456);
        var initialLimitAmount = 1000m;
        var initialLimitDescription = "Initial Limit";

        // Act
        var result = Customer.Create(documentNumber, name, email, account, initialLimitAmount, initialLimitDescription);

        // Assert
        result.Should().NotBeNull();
        result.DocumentNumber.Should().Be(documentNumber);
        result.Name.Should().Be(name);
        result.Email.Should().Be("john.doe@email.com");
        result.Account.Should().Be(account);
        result.IsActive.Should().BeTrue();
        result.LimitEntries.Should().HaveCount(1);
        result.LimitEntries.First().Amount.Should().Be(initialLimitAmount);
        result.LimitEntries.First().Description.Should().Be(initialLimitDescription);
        result.CurrentLimit.Should().Be(initialLimitAmount);
        result.DomainEvents.Should().HaveCount(1);
        result.DomainEvents.Should().ContainSingle(e => e is CustomerCreatedDomainEvent);
    }

    [Fact]
    public void Create_WhenEmailHasUpperCase_ShouldConvertToLowerCase()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "John Doe";
        var email = "JOHN.DOE@EMAIL.COM";
        var account = new Account(Agency.Agency1, 123456);

        // Act
        var result = Customer.Create(documentNumber, name, email, account, 1000m, "Initial");

        // Assert
        result.Email.Should().Be("john.doe@email.com");
    }

    [Fact]
    public void Create_WhenNameHasWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "  John Doe  ";
        var email = "john.doe@email.com";
        var account = new Account(Agency.Agency1, 123456);

        // Act
        var result = Customer.Create(documentNumber, name, email, account, 1000m, "Initial");

        // Assert
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public void Create_WhenZeroInitialLimit_ShouldNotCreateLimitEntry()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "John Doe";
        var email = "john.doe@email.com";
        var account = new Account(Agency.Agency1, 123456);

        // Act
        var result = Customer.Create(documentNumber, name, email, account, 0m, "Initial");

        // Assert
        result.LimitEntries.Should().BeEmpty();
        result.CurrentLimit.Should().Be(0m);
    }

    [Fact]
    public void Create_WhenDocumentNumberIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var name = "John Doe";
        var email = "john.doe@email.com";
        var account = new Account(Agency.Agency1, 123456);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            Customer.Create(null!, name, email, account, 1000m, "Initial"));
        exception.ParamName.Should().Be("documentNumber");
    }

    [Fact]
    public void Create_WhenAccountIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "John Doe";
        var email = "john.doe@email.com";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            Customer.Create(documentNumber, name, email, null!, 1000m, "Initial"));
        exception.ParamName.Should().Be("account");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsInvalid_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var email = "john.doe@email.com";
        var account = new Account(Agency.Agency1, 123456);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Customer.Create(documentNumber, invalidName, email, account, 1000m, "Initial"));
        exception.ParamName.Should().Be("name");
        exception.Message.Should().Contain("Customer name cannot be null or empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenEmailIsInvalid_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var documentNumber = new DocumentNumber("11144477735");
        var name = "John Doe";
        var account = new Account(Agency.Agency1, 123456);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Customer.Create(documentNumber, name, invalidEmail, account, 1000m, "Initial"));
        exception.ParamName.Should().Be("email");
        exception.Message.Should().Contain("Customer email cannot be null or empty");
    }

    #endregion

    #region AdjustLimit Tests

    [Fact]
    public void AdjustLimit_WhenPositiveAmount_ShouldAddLimitEntryAndUpdateTotal()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var initialLimit = customer.CurrentLimit;
        var adjustmentAmount = 500m;
        var description = "Limit increase";

        // Act
        customer.AdjustLimit(adjustmentAmount, description);

        // Assert
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(adjustmentAmount);
        customer.LimitEntries.Last().Description.Should().Be(description);
        customer.CurrentLimit.Should().Be(initialLimit + adjustmentAmount);
        
        var limitUpdatedEvent = customer.DomainEvents.OfType<CustomerLimitUpdatedDomainEvent>().FirstOrDefault();
        limitUpdatedEvent.Should().NotBeNull();
        limitUpdatedEvent!.CustomerId.Should().Be(customer.Id);
        limitUpdatedEvent.CurrentTotalLimit.Should().Be(customer.CurrentLimit);
    }

    [Fact]
    public void AdjustLimit_WhenNegativeAmount_ShouldAddLimitEntryAndUpdateTotal()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var initialLimit = customer.CurrentLimit;
        var adjustmentAmount = -200m;
        var description = "Limit decrease";

        // Act
        customer.AdjustLimit(adjustmentAmount, description);

        // Assert
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(adjustmentAmount);
        customer.CurrentLimit.Should().Be(initialLimit + adjustmentAmount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AdjustLimit_WhenInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => customer.AdjustLimit(100m, invalidDescription));
        exception.ParamName.Should().Be("description");
        exception.Message.Should().Contain("Limit adjustment description cannot be null or empty");
    }

    [Fact]
    public void AdjustLimit_WhenCustomerIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.Deactivate("Test deactivation");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => customer.AdjustLimit(100m, "Test"));
        exception.Message.Should().Be("Cannot perform operations on an inactive customer.");
    }

    #endregion

    #region IncreaseLimit Tests

    [Fact]
    public void IncreaseLimit_WhenPositiveAmount_ShouldAddPositiveLimitEntry()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var initialLimit = customer.CurrentLimit;
        var increaseAmount = 500m;
        var description = "Credit limit increase";

        // Act
        customer.IncreaseLimit(increaseAmount, description);

        // Assert
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(increaseAmount);
        customer.CurrentLimit.Should().Be(initialLimit + increaseAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void IncreaseLimit_WhenNonPositiveAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            customer.IncreaseLimit(invalidAmount, "Test"));
        exception.ParamName.Should().Be("amount");
        exception.Message.Should().Contain("Increase amount must be positive");
    }

    #endregion

    #region DecreaseLimit Tests

    [Fact]
    public void DecreaseLimit_WhenPositiveAmount_ShouldAddNegativeLimitEntry()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var initialLimit = customer.CurrentLimit;
        var decreaseAmount = 200m;
        var description = "Credit limit decrease";

        // Act
        customer.DecreaseLimit(decreaseAmount, description);

        // Assert
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(-decreaseAmount);
        customer.CurrentLimit.Should().Be(initialLimit - decreaseAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void DecreaseLimit_WhenNonPositiveAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            customer.DecreaseLimit(invalidAmount, "Test"));
        exception.ParamName.Should().Be("amount");
        exception.Message.Should().Contain("Decrease amount must be positive");
    }

    #endregion

    #region UpdateAccount Tests

    [Fact]
    public void UpdateAccount_WhenValidAccount_ShouldUpdateAccountAndRaiseEvent()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var newAccount = new Account(Agency.Agency2, 654321);

        // Act
        customer.UpdateAccount(newAccount);

        // Assert
        customer.Account.Should().Be(newAccount);
        
        var accountUpdatedEvent = customer.DomainEvents.OfType<CustomerAccountUpdatedDomainEvent>().FirstOrDefault();
        accountUpdatedEvent.Should().NotBeNull();
        accountUpdatedEvent!.CustomerId.Should().Be(customer.Id);
        accountUpdatedEvent.Account.Should().Be(newAccount);
    }

    [Fact]
    public void UpdateAccount_WhenSameAccount_ShouldNotRaiseEvent()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var sameAccount = customer.Account;
        var initialEventCount = customer.DomainEvents.Count;

        // Act
        customer.UpdateAccount(sameAccount);

        // Assert
        customer.Account.Should().Be(sameAccount);
        customer.DomainEvents.Should().HaveCount(initialEventCount);
    }

    [Fact]
    public void UpdateAccount_WhenAccountIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => customer.UpdateAccount(null!));
        exception.ParamName.Should().Be("account");
    }

    [Fact]
    public void UpdateAccount_WhenCustomerIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.Deactivate("Test deactivation");
        var newAccount = new Account(Agency.Agency2, 654321);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => customer.UpdateAccount(newAccount));
        exception.Message.Should().Contain("Cannot perform operations on an inactive customer.");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenValidReason_ShouldDeactivateCustomerAndRaiseEvent()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var reason = "Account closure requested";
        var deactivatedBy = "admin@bank.com";

        // Act
        customer.Deactivate(reason, deactivatedBy);

        // Assert
        customer.IsActive.Should().BeFalse();
        
        var deactivatedEvent = customer.DomainEvents.OfType<CustomerDeactivatedDomainEvent>().FirstOrDefault();
        deactivatedEvent.Should().NotBeNull();
        deactivatedEvent!.CustomerId.Should().Be(customer.Id);
        deactivatedEvent.Reason.Should().Be(reason);
        deactivatedEvent.DeactivatedBy.Should().Be(deactivatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deactivate_WhenInvalidReason_ShouldThrowArgumentException(string invalidReason)
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => customer.Deactivate(invalidReason));
        exception.ParamName.Should().Be("reason");
        exception.Message.Should().Contain("Deactivation reason cannot be null or empty");
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.Deactivate("First deactivation");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => customer.Deactivate("Second deactivation"));
        exception.Message.Should().Contain("Customer is already inactive");
    }

    #endregion

    #region CurrentLimit Tests

    [Fact]
    public void CurrentLimit_WhenMultipleLimitEntries_ShouldReturnSum()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.AdjustLimit(500m, "Increase 1");
        customer.AdjustLimit(-200m, "Decrease 1");
        customer.AdjustLimit(300m, "Increase 2");

        // Act
        var result = customer.CurrentLimit;

        // Assert
        result.Should().Be(1000m + 500m - 200m + 300m);
    }

    [Fact]
    public void CurrentLimit_WhenNoLimitEntries_ShouldReturnZero()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            0m,
            "No limit");

        // Act
        var result = customer.CurrentLimit;

        // Assert
        result.Should().Be(0m);
    }

    #endregion

    #region Helper Methods

    private static Customer CreateValidCustomer()
    {
        return Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");
    }

    #endregion
}
