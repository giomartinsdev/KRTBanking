using KRTBanking.Application.DTOs.Customer;
using KRTBanking.Application.DTOs.Transaction;
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
    public async Task<PagedCustomersDto> GetCustomersAsync(int pageSize = 10, string? lastEvaluatedKey = null, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be between 1 and 100");
        }

        _logger.LogInformation("Getting customers with page size: {PageSize}", pageSize);

        var (customers, nextPageKey) = await _customerRepository.GetAllAsync(pageSize, lastEvaluatedKey, includeInactive: false, cancellationToken);

        var customerDtos = customers.Select(MapToDto).ToList();

        return new PagedCustomersDto
        {
            Customers = customerDtos,
            NextPageKey = nextPageKey,
            HasNextPage = !string.IsNullOrEmpty(nextPageKey)
        };
    }

    /// <inheritdoc />
    public async Task DeactivateCustomerAsync(Guid customerId, DeactivateCustomerDto deactivateDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deactivateDto);
        
        _logger.LogInformation("Deactivating customer with ID: {CustomerId}, Reason: {Reason}", customerId, deactivateDto.Reason);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new InvalidOperationException($"Customer with ID {customerId} not found");
        }

        customer.Deactivate(deactivateDto.Reason, deactivateDto.DeactivatedBy);
        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Customer deactivated successfully with ID: {CustomerId}", customerId);
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
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Version = customer.Version
        };
    }

    /// <inheritdoc />
    public async Task<TransactionResultDto> ExecuteTransactionAsync(ExecuteTransactionDto executeTransactionDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(executeTransactionDto);

        _logger.LogInformation("Executing transaction for merchant document: {MerchantDocument}, Value: {Value}", 
            executeTransactionDto.MerchantDocument, executeTransactionDto.Value);

        var customer = await _customerRepository.GetByDocumentNumberAsync(executeTransactionDto.MerchantDocument, cancellationToken);
        
        if (customer is null)
        {
            _logger.LogWarning("Customer not found with document number: {MerchantDocument}", executeTransactionDto.MerchantDocument);
            return new TransactionResultDto
            {
                IsAuthorized = false,
                Reason = "Customer not found",
                TransactionValue = executeTransactionDto.Value
            };
        }

        if (!customer.IsActive)
        {
            _logger.LogWarning("Transaction denied - Customer is inactive: {CustomerId}", customer.Id);
            return new TransactionResultDto
            {
                IsAuthorized = false,
                Reason = "Customer account is inactive",
                TransactionValue = executeTransactionDto.Value,
                RemainingLimit = customer.CurrentLimit
            };
        }

        if (executeTransactionDto.Value <= customer.CurrentLimit)
        {
            customer.AdjustLimit(-executeTransactionDto.Value, $"Transaction for value {executeTransactionDto.Value:C}");
            
            await _customerRepository.UpdateAsync(customer, cancellationToken);

            _logger.LogInformation("Transaction authorized for customer: {CustomerId}, Amount: {Amount}, Remaining limit: {RemainingLimit}", 
                customer.Id, executeTransactionDto.Value, customer.CurrentLimit);

            return new TransactionResultDto
            {
                IsAuthorized = true,
                Reason = "Transaction authorized",
                TransactionValue = executeTransactionDto.Value,
                RemainingLimit = customer.CurrentLimit
            };
        }
        else
        {
            _logger.LogWarning("Transaction denied - Insufficient limit. Customer: {CustomerId}, Required: {Required}, Available: {Available}", 
                customer.Id, executeTransactionDto.Value, customer.CurrentLimit);

            return new TransactionResultDto
            {
                IsAuthorized = false,
                Reason = "Insufficient limit",
                TransactionValue = executeTransactionDto.Value,
                RemainingLimit = customer.CurrentLimit
            };
        }
    }
}
