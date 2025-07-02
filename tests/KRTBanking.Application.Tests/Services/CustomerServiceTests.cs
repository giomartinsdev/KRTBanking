using FluentAssertions;
using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.DTOs.Transaction;
using KRTBanking.Application.Services;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.Repositories;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace KRTBanking.Application.Tests.Services;

/// <summary>
/// Tests for CustomerService using AAA pattern.
/// Tests cover service operations with mocked dependencies.
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _service = new CustomerService(_mockRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new CustomerService(_mockRepository.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new CustomerService(null!, _mockLogger.Object));
        exception.ParamName.Should().Be("customerRepository");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new CustomerService(_mockRepository.Object, null!));
        exception.ParamName.Should().Be("logger");
    }

    #endregion

    #region CreateCustomerAsync Tests

    [Fact]
    public async Task CreateCustomerAsync_WhenValidDto_ShouldCreateAndReturnCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Agency = Agency.Agency1,
            AccountNumber = 123456,
            LimitAmount = 1000m,
            LimitDescription = "Initial Limit"
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(createDto.DocumentNumber, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.DocumentNumber.Should().Be(createDto.DocumentNumber);
        result.Name.Should().Be(createDto.Name);
        result.Email.Should().Be(createDto.Email.ToLowerInvariant());
        result.Account.Agency.Should().Be(createDto.Agency);
        result.Account.AccountNumber.Should().Be(createDto.AccountNumber);
        result.CurrentLimit.Should().Be(createDto.LimitAmount);
        result.IsActive.Should().BeTrue();

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(createDto.DocumentNumber, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WhenCustomerAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            DocumentNumber = "11144477735",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Agency = Agency.Agency1,
            AccountNumber = 123456,
            LimitAmount = 1000m,
            LimitDescription = "Initial Limit"
        };

        var existingCustomer = Customer.Create(
            new DocumentNumber(createDto.DocumentNumber),
            "Existing Customer",
            "existing@email.com",
            new Account(Agency.Agency2, 654321),
            500m,
            "Existing Limit");

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(createDto.DocumentNumber, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateCustomerAsync(createDto));
        
        exception.Message.Should().Contain($"Customer with document number {createDto.DocumentNumber} already exists");
        
        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(createDto.DocumentNumber, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCustomerAsync_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.CreateCustomerAsync(null!));
        exception.ParamName.Should().Be("createCustomerDto");
    }

    [Fact]
    public async Task CreateCustomerAsync_WhenInvalidDocumentNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            DocumentNumber = "invalid-cpf",
            Name = "John Doe",
            Email = "john.doe@email.com",
            Agency = Agency.Agency1,
            AccountNumber = 123456,
            LimitAmount = 1000m,
            LimitDescription = "Initial Limit"
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(createDto.DocumentNumber, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreateCustomerAsync(createDto));
        
        exception.Message.Should().Contain("Invalid document number format");
    }

    #endregion

    #region GetCustomerByIdAsync Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        // Use reflection to set the ID
        var customerType = typeof(Customer);
        var idProperty = customerType.BaseType?.GetProperty("Id");
        idProperty?.SetValue(customer, customerId);

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        // Act
        var result = await _service.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.DocumentNumber.Should().Be("11144477735");
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john.doe@email.com");
        result.Account.Agency.Should().Be(Agency.Agency1);
        result.Account.AccountNumber.Should().Be(123456);
        result.CurrentLimit.Should().Be(1000m);
        result.IsActive.Should().BeTrue();

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().BeNull();

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AdjustCustomerLimitAsync Tests

    [Fact]
    public async Task AdjustCustomerLimitAsync_WhenValidDto_ShouldAdjustLimitAndReturnCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        // Use reflection to set the ID
        var customerType = typeof(Customer);
        var idProperty = customerType.BaseType?.GetProperty("Id");
        idProperty?.SetValue(customer, customerId);

        var adjustDto = new AdjustLimitDto
        {
            Amount = 500m,
            Description = "Limit increase"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AdjustCustomerLimitAsync(customerId, adjustDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.CurrentLimit.Should().Be(1500m); // 1000 + 500
        result.LimitEntries.Should().HaveCount(2); // Initial + adjustment

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_WhenCustomerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustDto = new AdjustLimitDto
        {
            Amount = 500m,
            Description = "Limit increase"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.AdjustCustomerLimitAsync(customerId, adjustDto));
        
        exception.Message.Should().Contain($"Customer with ID {customerId} not found");

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.AdjustCustomerLimitAsync(customerId, null!));
        exception.ParamName.Should().Be("adjustLimitDto");
    }

    #endregion

    #region DeactivateCustomerAsync Tests

    [Fact]
    public async Task DeactivateCustomerAsync_WhenValidDto_ShouldDeactivateCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        // Use reflection to set the ID
        var customerType = typeof(Customer);
        var idProperty = customerType.BaseType?.GetProperty("Id");
        idProperty?.SetValue(customer, customerId);

        var deactivateDto = new DeactivateCustomerDto
        {
            Reason = "Customer requested account closure",
            DeactivatedBy = "admin@bank.com"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        await _service.DeactivateCustomerAsync(customerId, deactivateDto);

        // Assert
        customer.IsActive.Should().BeFalse();

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateCustomerAsync_WhenCustomerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var deactivateDto = new DeactivateCustomerDto
        {
            Reason = "Account closure",
            DeactivatedBy = "admin@bank.com"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.DeactivateCustomerAsync(customerId, deactivateDto));
        
        exception.Message.Should().Contain($"Customer with ID {customerId} not found");

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateCustomerAsync_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.DeactivateCustomerAsync(customerId, null!));
        exception.ParamName.Should().Be("deactivateDto");
    }

    [Fact]
    public async Task DeactivateCustomerAsync_WhenAlreadyInactive_ShouldFail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        var customerType = typeof(Customer);
        var idProperty = customerType.BaseType?.GetProperty("Id");
        idProperty?.SetValue(customer, customerId);

        // Deactivate customer first
        customer.Deactivate("Previous deactivation", "system");

        var deactivateDto = new DeactivateCustomerDto
        {
            Reason = "Duplicate deactivation request",
            DeactivatedBy = "admin@bank.com"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.DeactivateCustomerAsync(customerId, deactivateDto));
        
        exception.Message.Should().Contain($"Customer is already inactive.");
        customer.IsActive.Should().BeFalse();

        _mockRepository.Verify(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ExecuteTransactionAsync Tests

    [Fact]
    public async Task ExecuteTransactionAsync_WhenValidTransaction_ShouldAuthorizeAndAdjustLimit()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "11144477735",
            Value = 500m
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeTrue();
        result.Reason.Should().Be("Transaction authorized");
        result.TransactionValue.Should().Be(500m);
        result.RemainingLimit.Should().Be(500m); // 1000 - 500
        
        customer.CurrentLimit.Should().Be(500m);

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenInsufficientLimit_ShouldDenyTransaction()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "11144477735",
            Value = 1500m // Exceeds available limit
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeFalse();
        result.Reason.Should().Be("Insufficient limit");
        result.TransactionValue.Should().Be(1500m);
        result.RemainingLimit.Should().Be(1000m); // Unchanged
        
        customer.CurrentLimit.Should().Be(1000m); // Should remain unchanged

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenCustomerNotFound_ShouldDenyTransaction()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "99999999999",
            Value = 500m
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeFalse();
        result.Reason.Should().Be("Customer not found");
        result.TransactionValue.Should().Be(500m);
        result.RemainingLimit.Should().BeNull();

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenCustomerInactive_ShouldDenyTransaction()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        // Deactivate the customer
        customer.Deactivate("Account suspended", "admin");

        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "11144477735",
            Value = 500m
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeFalse();
        result.Reason.Should().Be("Customer account is inactive");
        result.TransactionValue.Should().Be(500m);
        result.RemainingLimit.Should().Be(1000m);

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.ExecuteTransactionAsync(null!));
        exception.ParamName.Should().Be("executeTransactionDto");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenExactLimitAmount_ShouldAuthorizeAndExhaustLimit()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "11144477735",
            Value = 1000m // Exact limit amount
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeTrue();
        result.Reason.Should().Be("Transaction authorized");
        result.TransactionValue.Should().Be(1000m);
        result.RemainingLimit.Should().Be(0m); // Limit exhausted
        
        customer.CurrentLimit.Should().Be(0m);

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenSmallTransaction_ShouldAuthorizeCorrectly()
    {
        // Arrange
        var customer = Customer.Create(
            new DocumentNumber("11144477735"),
            "John Doe",
            "john.doe@email.com",
            new Account(Agency.Agency1, 123456),
            1000m,
            "Initial Limit");

        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "11144477735",
            Value = 0.01m // Minimum transaction value
        };

        _mockRepository.Setup(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(customer);

        _mockRepository.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAuthorized.Should().BeTrue();
        result.Reason.Should().Be("Transaction authorized");
        result.TransactionValue.Should().Be(0.01m);
        result.RemainingLimit.Should().Be(999.99m);
        
        customer.CurrentLimit.Should().Be(999.99m);

        _mockRepository.Verify(r => r.GetByDocumentNumberAsync(executeDto.MerchantDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetCustomersAsync Tests

    [Fact]
    public async Task GetCustomersAsync_WhenValidParameters_ShouldReturnPagedCustomers()
    {
        // Arrange
        var customer1 = Customer.Create(
            new DocumentNumber("11144477735"), 
            "Customer 1", 
            "customer1@email.com", 
            new Account(Agency.Agency1, 123456), 
            1000m, 
            "Initial Limit");

        var customer2 = Customer.Create(
            new DocumentNumber("52998224725"), 
            "Customer 2", 
            "customer2@email.com", 
            new Account(Agency.Agency2, 654321), 
            2000m, 
            "Initial Limit");

        var customers = new List<Customer> { customer1, customer2 };

        _mockRepository.Setup(r => r.GetAllAsync(10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, "next-page-key"));

        // Act
        var result = await _service.GetCustomersAsync(10, null);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(2);
        
        var firstCustomer = result.Customers.First();
        firstCustomer.DocumentNumber.Should().Be("11144477735");
        firstCustomer.Name.Should().Be("Customer 1");
        firstCustomer.Email.Should().Be("customer1@email.com");
        firstCustomer.Account.Agency.Should().Be(Agency.Agency1);
        firstCustomer.Account.AccountNumber.Should().Be(123456);
        firstCustomer.CurrentLimit.Should().Be(1000m);
        firstCustomer.IsActive.Should().BeTrue();
        
        var secondCustomer = result.Customers.Last();
        secondCustomer.DocumentNumber.Should().Be("52998224725");
        secondCustomer.Name.Should().Be("Customer 2");
        secondCustomer.Email.Should().Be("customer2@email.com");
        secondCustomer.Account.Agency.Should().Be(Agency.Agency2);
        secondCustomer.Account.AccountNumber.Should().Be(654321);
        secondCustomer.CurrentLimit.Should().Be(2000m);
        secondCustomer.IsActive.Should().BeTrue();
        
        result.NextPageKey.Should().Be("next-page-key");
        result.HasNextPage.Should().BeTrue();

        _mockRepository.Verify(r => r.GetAllAsync(10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersAsync_WhenNoCustomers_ShouldReturnEmptyResult()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Customer>(), (string?)null));

        // Act
        var result = await _service.GetCustomersAsync(10, null);

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().BeEmpty();
        result.NextPageKey.Should().BeNull();
        result.HasNextPage.Should().BeFalse();

        _mockRepository.Verify(r => r.GetAllAsync(10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersAsync_WhenLastPageReached_ShouldIndicateNoNextPage()
    {
        // Arrange
        var customers = new List<Customer>
        {
            Customer.Create(new DocumentNumber("11144477735"), "Customer 1", "customer1@email.com", new Account(Agency.Agency1, 123456), 1000m, "Initial")
        };

        _mockRepository.Setup(r => r.GetAllAsync(10, "some-page-key", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, (string?)null));

        // Act
        var result = await _service.GetCustomersAsync(10, "some-page-key");

        // Assert
        result.Should().NotBeNull();
        result.Customers.Should().HaveCount(1);
        result.NextPageKey.Should().BeNull();
        result.HasNextPage.Should().BeFalse();

        _mockRepository.Verify(r => r.GetAllAsync(10, "some-page-key", false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersAsync_WhenInvalidPageSize_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _service.GetCustomersAsync(0, null));
        
        exception.Message.Should().Contain("Page size must be between 1 and 100");
        exception.ParamName.Should().Be("pageSize");
    }

    [Fact]
    public async Task GetCustomersAsync_WhenNegativePageSize_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _service.GetCustomersAsync(-1, null));
        
        exception.Message.Should().Contain("Page size must be between 1 and 100");
        exception.ParamName.Should().Be("pageSize");
    }

    [Fact]
    public async Task GetCustomersAsync_WhenPageSizeTooLarge_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            _service.GetCustomersAsync(101, null));
        
        exception.Message.Should().Contain("Page size must be between 1 and 100");
        exception.ParamName.Should().Be("pageSize");
    }

    #endregion

    #region Helper Classes

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public string? NextPageKey { get; }
        public bool HasMoreResults { get; }

        public PagedResult(IReadOnlyList<T> items, string? nextPageKey, bool hasMoreResults)
        {
            Items = items;
            NextPageKey = nextPageKey;
            HasMoreResults = hasMoreResults;
        }
    }

    #endregion
}
