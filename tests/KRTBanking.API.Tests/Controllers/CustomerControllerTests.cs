using FluentAssertions;
using KRTBanking.API.Controllers;
using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.DTOs.Transaction;
using KRTBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRTBanking.API.Tests.Controllers;

public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<ILogger<CustomerController>> _loggerMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _customerServiceMock = new Mock<ICustomerService>();
        _loggerMock = new Mock<ILogger<CustomerController>>();
        _controller = new CustomerController(_customerServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullCustomerService_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CustomerController(null!, _loggerMock.Object));
    }

    [Fact]
    public async Task CreateCustomerAsync_ValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            DocumentNumber = "12345678901",
            Name = "John Doe",
            Email = "john@example.com",
            AccountNumber = 12345,
            LimitAmount = 5000m
        };

        var customerDto = new CustomerDto
        {
            Id = Guid.NewGuid(),
            DocumentNumber = createDto.DocumentNumber,
            Name = createDto.Name,
            Email = createDto.Email
        };

        _customerServiceMock
            .Setup(x => x.CreateCustomerAsync(It.IsAny<CreateCustomerDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        // Act
        var result = await _controller.CreateCustomerAsync(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtRouteResult>();
        var createdResult = result.Result as CreatedAtRouteResult;
        createdResult!.RouteName.Should().Be("GetCustomerById");
        createdResult.RouteValues!["id"].Should().Be(customerDto.Id);
        createdResult.Value.Should().Be(customerDto);
    }

    [Fact]
    public async Task CreateCustomerAsync_CustomerExists_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            DocumentNumber = "12345678901",
            Name = "John Doe"
        };

        _customerServiceMock
            .Setup(x => x.CreateCustomerAsync(It.IsAny<CreateCustomerDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer already exists"));

        // Act
        var result = await _controller.CreateCustomerAsync(createDto);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result.Result as ConflictObjectResult;
        var problemDetails = conflictResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Customer Already Exists");
    }

    [Fact]
    public async Task CreateCustomerAsync_InvalidArgument_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto();

        _customerServiceMock
            .Setup(x => x.CreateCustomerAsync(It.IsAny<CreateCustomerDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid document number"));

        // Act
        var result = await _controller.CreateCustomerAsync(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid Request");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ExistingCustomer_ShouldReturnOk()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDto = new CustomerDto
        {
            Id = customerId,
            DocumentNumber = "12345678901",
            Name = "John Doe"
        };

        _customerServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        // Act
        var result = await _controller.GetCustomerByIdAsync(customerId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(customerDto);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_NonExistingCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomerByIdAsync(customerId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        var problemDetails = notFoundResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Customer Not Found");
    }

    [Fact]
    public async Task GetCustomersAsync_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var pagedCustomers = new PagedCustomersDto
        {
            Customers = new List<CustomerDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Customer 1" },
                new() { Id = Guid.NewGuid(), Name = "Customer 2" }
            },
            HasNextPage = false
        };

        _customerServiceMock
            .Setup(x => x.GetCustomersAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedCustomers);

        // Act
        var result = await _controller.GetCustomersAsync();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(pagedCustomers);
    }

    [Fact]
    public async Task GetCustomersAsync_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        _customerServiceMock
            .Setup(x => x.GetCustomersAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentOutOfRangeException("pageSize", "Page size must be between 1 and 100"));

        // Act
        var result = await _controller.GetCustomersAsync(pageSize: 0);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid Page Size");
    }

    [Fact]
    public async Task DeleteCustomerAsync_ExistingCustomer_ShouldReturnNoContent()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerServiceMock
            .Setup(x => x.DeactivateCustomerAsync(customerId, It.IsAny<DeactivateCustomerDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify correct deactivation DTO was passed
        _customerServiceMock.Verify(x => x.DeactivateCustomerAsync(
            customerId,
            It.Is<DeactivateCustomerDto>(dto => 
                dto.Reason == "Customer deletion requested via API" &&
                dto.DeactivatedBy == "API User"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_NonExistingCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerServiceMock
            .Setup(x => x.DeactivateCustomerAsync(customerId, It.IsAny<DeactivateCustomerDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer not found"));

        // Act
        var result = await _controller.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        var problemDetails = notFoundResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Customer Not Found");
    }

    [Fact]
    public async Task DeleteCustomerAsync_AlreadyInactive_ShouldReturnConflict()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerServiceMock
            .Setup(x => x.DeactivateCustomerAsync(customerId, It.IsAny<DeactivateCustomerDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer already inactive"));

        // Act
        var result = await _controller.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        var problemDetails = conflictResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Customer Already Deleted");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_AuthorizedTransaction_ShouldReturnOk()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 500m
        };

        var resultDto = new TransactionResultDto
        {
            IsAuthorized = true,
            Reason = "Transaction approved",
            RemainingLimit = 1500m,
            TransactionValue = 500m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(resultDto);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_DeniedTransaction_ShouldReturnOk()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 5000m
        };

        var resultDto = new TransactionResultDto
        {
            IsAuthorized = false,
            Reason = "Insufficient limit",
            RemainingLimit = null,
            TransactionValue = 5000m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(resultDto);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_ArgumentNullException_ShouldReturnBadRequest()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 500m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException("executeTransactionDto"));

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid Request");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_ArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "invalid",
            Value = 500m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid merchant document"));

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Validation Error");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_InvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 500m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer is inactive"));

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Business Rule Violation");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_UnexpectedException_ShouldReturnInternalServerError()
    {
        // Arrange
        var executeDto = new ExecuteTransactionDto
        {
            MerchantDocument = "12345678901",
            Value = 500m
        };

        _customerServiceMock
            .Setup(x => x.ExecuteTransactionAsync(It.IsAny<ExecuteTransactionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.ExecuteTransactionAsync(executeDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().Be("Internal Server Error");
    }
}
