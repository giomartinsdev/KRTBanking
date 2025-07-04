using FluentAssertions;
using KRTBanking.API.Controllers;
using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRTBanking.API.Tests.Controllers;

public class LimitControllerTests
{
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<ILogger<LimitController>> _loggerMock;
    private readonly LimitController _controller;

    public LimitControllerTests()
    {
        _customerServiceMock = new Mock<ICustomerService>();
        _loggerMock = new Mock<ILogger<LimitController>>();
        _controller = new LimitController(_customerServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new LimitController(_customerServiceMock.Object, null!));
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 1000m,
            Description = "Credit increase"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            DocumentNumber = "12345678901",
            Name = "John Doe",
            LimitEntries = new List<LimitEntryDto>
            {
                new() { Amount = 5000m, Description = "Initial limit" },
                new() { Amount = 1000m, Description = "Credit increase" }
            }
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(updatedCustomer);

        _customerServiceMock.Verify(x => x.AdjustCustomerLimitAsync(
            customerId, 
            adjustLimitDto, 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_CustomerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 1000m,
            Description = "Credit increase"
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer not found"));

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        var problemDetails = notFoundResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Customer Not Found");
        problemDetails.Detail.Should().Be("Customer not found");
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_InvalidArgument_ShouldReturnBadRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 0m,
            Description = ""
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Adjustment amount cannot be zero"));

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ProblemDetails;
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid Request");
        problemDetails.Detail.Should().Be("Adjustment amount cannot be zero");
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_PositiveAdjustment_ShouldIncreaseLimit()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 2500m,
            Description = "Salary increase adjustment"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            DocumentNumber = "12345678901",
            Name = "John Doe",
            LimitEntries = new List<LimitEntryDto>
            {
                new() { Amount = 5000m, Description = "Initial limit" },
                new() { Amount = 2500m, Description = "Salary increase adjustment" }
            }
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var customer = okResult!.Value as CustomerDto;
        customer!.CurrentLimit.Should().Be(7500m);
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(2500m);
        customer.LimitEntries.Last().Description.Should().Be("Salary increase adjustment");
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_NegativeAdjustment_ShouldDecreaseLimit()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = -1500m,
            Description = "Risk adjustment"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            DocumentNumber = "12345678901",
            Name = "John Doe",
            LimitEntries = new List<LimitEntryDto>
            {
                new() { Amount = 5000m, Description = "Initial limit" },
                new() { Amount = -1500m, Description = "Risk adjustment" }
            }
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var customer = okResult!.Value as CustomerDto;
        customer!.CurrentLimit.Should().Be(3500m);
        customer.LimitEntries.Should().HaveCount(2);
        customer.LimitEntries.Last().Amount.Should().Be(-1500m);
        customer.LimitEntries.Last().Description.Should().Be("Risk adjustment");
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_EmptyGuid_ShouldProcessNormally()
    {
        // Arrange
        var customerId = Guid.Empty;
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 1000m,
            Description = "Test adjustment"
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Customer not found"));

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _customerServiceMock.Verify(x => x.AdjustCustomerLimitAsync(
            customerId, 
            adjustLimitDto, 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_LargePositiveAmount_ShouldSucceed()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 100000m,
            Description = "Large corporate limit increase"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            DocumentNumber = "12345678901",
            Name = "Corporate Client",
            LimitEntries = new List<LimitEntryDto>
            {
                new() { Amount = 50000m, Description = "Initial corporate limit" },
                new() { Amount = 100000m, Description = "Large corporate limit increase" }
            }
        };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var customer = okResult!.Value as CustomerDto;
        customer!.CurrentLimit.Should().Be(150000m);
    }

    [Fact]
    public async Task AdjustCustomerLimitAsync_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var adjustLimitDto = new AdjustLimitDto
        {
            Amount = 1000m,
            Description = "Test"
        };
        var cancellationToken = new CancellationToken(true);

        var updatedCustomer = new CustomerDto { Id = customerId };

        _customerServiceMock
            .Setup(x => x.AdjustCustomerLimitAsync(customerId, adjustLimitDto, cancellationToken))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.AdjustCustomerLimitAsync(customerId, adjustLimitDto, cancellationToken);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _customerServiceMock.Verify(x => x.AdjustCustomerLimitAsync(
            customerId, 
            adjustLimitDto, 
            cancellationToken), Times.Once);
    }
}
