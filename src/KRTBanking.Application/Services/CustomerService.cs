using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.Interfaces.Services;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.Repositories;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using Microsoft.Extensions.Logging;

namespace KRTBanking.Application.Services;

/// <summary>
/// Service for managing customer-related business operations.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerService"/> class.
    /// </summary>
    /// <param name="customerRepository">The customer repository.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public CustomerService(
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createCustomerDto);

        _logger.LogInformation("Creating customer with document number: {DocumentNumber}", createCustomerDto.DocumentNumber);

        var existingCustomer = await _customerRepository.GetByDocumentNumberAsync(createCustomerDto.DocumentNumber, cancellationToken);
        if (existingCustomer is not null)
        {
            throw new InvalidOperationException($"Customer with document number {createCustomerDto.DocumentNumber} already exists");
        }

        var documentNumber = new DocumentNumber(createCustomerDto.DocumentNumber);
        var account = new Account(createCustomerDto.Agency, createCustomerDto.AccountNumber);

        var customer = Customer.Create(
            documentNumber,
            createCustomerDto.Name,
            createCustomerDto.Email,
            account,
            createCustomerDto.LimitAmount,
            createCustomerDto.LimitDescription);

        await _customerRepository.AddAsync(customer, cancellationToken);

        _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

        return MapToDto(customer);
    }

    /// <inheritdoc />
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer with ID: {CustomerId}", customerId);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", customerId);
            return null;
        }

        return MapToDto(customer);
    }

    /// <inheritdoc />
    public async Task<CustomerDto> UpdateCustomerAccountAsync(Guid customerId, UpdateAccountDto updateAccountDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateAccountDto);

        _logger.LogInformation("Updating account for customer with ID: {CustomerId}", customerId);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException($"Customer with ID {customerId} not found");
        }

        var newAccount = new Account(updateAccountDto.Agency, updateAccountDto.AccountNumber);
        customer.UpdateAccount(newAccount);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Account updated successfully for customer with ID: {CustomerId}", customerId);

        return MapToDto(customer);
    }
    
    /// <inheritdoc />
    public async Task<PagedCustomersDto> GetCustomersAsync(int pageSize = 10, string? lastEvaluatedKey = null, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be between 1 and 100");
        }

        _logger.LogInformation("Getting customers with page size: {PageSize}", pageSize);

        var (customers, nextPageKey) = await _customerRepository.GetAllAsync(pageSize, lastEvaluatedKey, cancellationToken);

        var customerDtos = customers.Select(MapToDto).ToList();

        return new PagedCustomersDto
        {
            Customers = customerDtos,
            NextPageKey = nextPageKey,
            HasNextPage = !string.IsNullOrEmpty(nextPageKey)
        };
    }

    /// <inheritdoc />
    public async Task DeleteCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", customerId);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException($"Customer with ID {customerId} not found");
        }

        await _customerRepository.RemoveAsync(customer, cancellationToken);

        _logger.LogInformation("Customer deleted successfully with ID: {CustomerId}", customerId);
    }

    /// <inheritdoc />
    public async Task<CustomerDto> AdjustCustomerLimitAsync(Guid customerId, AdjustLimitDto adjustLimitDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adjustLimitDto);

        _logger.LogInformation("Adjusting limit for customer with ID: {CustomerId} by {Amount}", customerId, adjustLimitDto.Amount);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException($"Customer with ID {customerId} not found");
        }

        customer.AdjustLimit(adjustLimitDto.Amount, adjustLimitDto.Description);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Limit adjusted successfully for customer with ID: {CustomerId}. New total: {NewTotal}", customerId, customer.CurrentLimit);

        return MapToDto(customer);
    }

    /// <inheritdoc />
    /// <summary>
    /// Maps a customer domain entity to a DTO.
    /// </summary>
    /// <param name="customer">The customer domain entity.</param>
    /// <returns>A customer DTO.</returns>
    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            DocumentNumber = customer.DocumentNumber.Value,
            Name = customer.Name,
            Email = customer.Email,
            Account = new AccountDto
            {
                Agency = customer.Account.Agency,
                AccountNumber = customer.Account.AccountNumber,
                Number = customer.Account.Number,
                CreatedAt = customer.Account.CreatedAt
            },
            LimitEntries = customer.LimitEntries.Select(entry => new LimitEntryDto
            {
                Amount = entry.Amount,
                Description = entry.Description,
                CreatedAt = entry.CreatedAt
            }).ToList(),
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Version = customer.Version
        };
    }
}
