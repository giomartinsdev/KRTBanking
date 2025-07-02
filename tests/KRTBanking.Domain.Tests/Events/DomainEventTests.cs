using FluentAssertions;
using KRTBanking.Domain.Context.Customer.Events;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Tests.Events;

public class CustomerCreatedDomainEventTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var documentNumber = DocumentNumber.TryCreate("12345678901");
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new CustomerCreatedDomainEvent(customerId, documentNumber);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.EventType.Should().Be(nameof(CustomerCreatedDomainEvent));
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.DocumentNumber.Should().Be(documentNumber);
    }

    [Fact]
    public void EventType_ShouldReturnCorrectTypeName()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var documentNumber = DocumentNumber.TryCreate("12345678901");

        // Act
        var domainEvent = new CustomerCreatedDomainEvent(customerId, documentNumber);

        // Assert
        domainEvent.EventType.Should().Be("CustomerCreatedDomainEvent");
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var documentNumber = DocumentNumber.TryCreate("12345678901");

        // Act
        var event1 = new CustomerCreatedDomainEvent(customerId, documentNumber);
        var event2 = new CustomerCreatedDomainEvent(customerId, documentNumber);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void Constructor_ShouldSetOccurredAtToUtcTime()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var documentNumber = DocumentNumber.TryCreate("12345678901");
        var utcNow = DateTime.UtcNow;

        // Act
        var domainEvent = new CustomerCreatedDomainEvent(customerId, documentNumber);

        // Assert
        domainEvent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
        domainEvent.OccurredAt.Should().BeCloseTo(utcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var documentNumber = DocumentNumber.TryCreate("12345678901");

        // Act
        var domainEvent = new CustomerCreatedDomainEvent(customerId, documentNumber);

        // Assert
        // Properties should only have getters, not setters
        var idProperty = typeof(CustomerCreatedDomainEvent).GetProperty(nameof(CustomerCreatedDomainEvent.Id));
        var occurredAtProperty = typeof(CustomerCreatedDomainEvent).GetProperty(nameof(CustomerCreatedDomainEvent.OccurredAt));
        var eventTypeProperty = typeof(CustomerCreatedDomainEvent).GetProperty(nameof(CustomerCreatedDomainEvent.EventType));
        var customerIdProperty = typeof(CustomerCreatedDomainEvent).GetProperty(nameof(CustomerCreatedDomainEvent.CustomerId));
        var documentNumberProperty = typeof(CustomerCreatedDomainEvent).GetProperty(nameof(CustomerCreatedDomainEvent.DocumentNumber));

        idProperty!.CanWrite.Should().BeFalse();
        occurredAtProperty!.CanWrite.Should().BeFalse();
        eventTypeProperty!.CanWrite.Should().BeFalse();
        customerIdProperty!.CanWrite.Should().BeFalse();
        documentNumberProperty!.CanWrite.Should().BeFalse();
    }
}

public class CustomerLimitUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry>
        {
            LimitEntry.Create(1000m, "Initial limit"),
            LimitEntry.Create(500m, "Adjustment")
        };
        var currentTotalLimit = 1500m;
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, currentTotalLimit);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.EventType.Should().Be(nameof(CustomerLimitUpdatedDomainEvent));
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.NewLimitEntries.Should().BeEquivalentTo(limitEntries);
        domainEvent.CurrentTotalLimit.Should().Be(currentTotalLimit);
    }

    [Fact]
    public void EventType_ShouldReturnCorrectTypeName()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry> { LimitEntry.Create(1000m, "Test") };

        // Act
        var domainEvent = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, 1000m);

        // Assert
        domainEvent.EventType.Should().Be("CustomerLimitUpdatedDomainEvent");
    }

    [Fact]
    public void Constructor_EmptyLimitEntries_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry>();
        var currentTotalLimit = 0m;

        // Act
        var domainEvent = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, currentTotalLimit);

        // Assert
        domainEvent.NewLimitEntries.Should().BeEmpty();
        domainEvent.CurrentTotalLimit.Should().Be(0m);
    }

    [Fact]
    public void Constructor_NegativeCurrentTotalLimit_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry>
        {
            LimitEntry.Create(1000m, "Initial"),
            LimitEntry.Create(-1500m, "Reduction")
        };
        var currentTotalLimit = -500m;

        // Act
        var domainEvent = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, currentTotalLimit);

        // Assert
        domainEvent.CurrentTotalLimit.Should().Be(-500m);
        domainEvent.NewLimitEntries.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry> { LimitEntry.Create(1000m, "Test") };

        // Act
        var event1 = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, 1000m);
        var event2 = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, 1000m);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void NewLimitEntries_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var limitEntries = new List<LimitEntry> { LimitEntry.Create(1000m, "Test") };

        // Act
        var domainEvent = new CustomerLimitUpdatedDomainEvent(customerId, limitEntries, 1000m);

        // Assert
        domainEvent.NewLimitEntries.Should().BeAssignableTo<IReadOnlyList<LimitEntry>>();
        domainEvent.NewLimitEntries.Should().HaveCount(1);
    }
}

public class CustomerDeactivatedDomainEventTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var reason = "Fraudulent activity detected";
        var deactivatedBy = "admin@bank.com";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new CustomerDeactivatedDomainEvent(customerId, reason, deactivatedBy);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.EventType.Should().Be(nameof(CustomerDeactivatedDomainEvent));
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.Reason.Should().Be(reason);
        domainEvent.DeactivatedBy.Should().Be(deactivatedBy);
    }

    [Fact]
    public void EventType_ShouldReturnCorrectTypeName()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var domainEvent = new CustomerDeactivatedDomainEvent(customerId, "Test reason", "test@user.com");

        // Assert
        domainEvent.EventType.Should().Be("CustomerDeactivatedDomainEvent");
    }

    [Fact]
    public void Constructor_NullDeactivatedBy_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var reason = "System deactivation";

        // Act
        var domainEvent = new CustomerDeactivatedDomainEvent(customerId, reason, null);

        // Assert
        domainEvent.DeactivatedBy.Should().BeNull();
        domainEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Constructor_NullReason_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CustomerDeactivatedDomainEvent(customerId, null!, "user@test.com"));
    }

    [Fact]
    public void Constructor_EmptyReason_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var domainEvent = new CustomerDeactivatedDomainEvent(customerId, "", "user@test.com");

        // Assert
        domainEvent.Reason.Should().Be("");
    }
}

public class CustomerAccountUpdatedDomainEventTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = Account.Create(Agency.Agency2, 54321);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new CustomerAccountUpdatedDomainEvent(customerId, account);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.Id.Should().NotBe(Guid.Empty);
        domainEvent.OccurredAt.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredAt.Should().BeOnOrBefore(afterCreation);
        domainEvent.EventType.Should().Be(nameof(CustomerAccountUpdatedDomainEvent));
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.Account.Should().Be(account);
    }

    [Fact]
    public void EventType_ShouldReturnCorrectTypeName()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = Account.Create(Agency.Agency1, 12345);

        // Act
        var domainEvent = new CustomerAccountUpdatedDomainEvent(customerId, account);

        // Assert
        domainEvent.EventType.Should().Be("CustomerAccountUpdatedDomainEvent");
    }

    [Fact]
    public void Constructor_NullAccount_ShouldCreateEventCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var domainEvent = new CustomerAccountUpdatedDomainEvent(customerId, null);

        // Assert
        domainEvent.Account.Should().BeNull();
        domainEvent.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var account = Account.Create(Agency.Agency1, 12345);

        // Act
        var event1 = new CustomerAccountUpdatedDomainEvent(customerId, account);
        var event2 = new CustomerAccountUpdatedDomainEvent(customerId, account);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }
}
