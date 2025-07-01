using KRTBanking.Application.DTOs.Customer;

namespace KRTBanking.Application.Interfaces.Services;

/// <summary>
/// Defines the contract for customer-related business operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Creates a new customer asynchronously.
    /// </summary>
    /// <param name="createCustomerDto">The customer creation data transfer object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created customer DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when createCustomerDto is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when business rules are violated.</exception>
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by their unique identifier asynchronously.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the customer DTO if found; otherwise, null.</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    

    /// <summary>
    /// Adjusts a customer's limit by adding a new limit entry asynchronously.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="adjustLimitDto">The limit adjustment data transfer object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated customer DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when adjustLimitDto is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when customer is not found or business rules are violated.</exception>
    Task<CustomerDto> AdjustCustomerLimitAsync(Guid customerId, AdjustLimitDto adjustLimitDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all customers with pagination support asynchronously.
    /// </summary>
    /// <param name="pageSize">The number of customers per page.</param>
    /// <param name="lastEvaluatedKey">The last evaluated key for pagination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paginated customers.</returns>
    Task<PagedCustomersDto> GetCustomersAsync(int pageSize = 10, string? lastEvaluatedKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer asynchronously.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when customer is not found.</exception>
    Task DeleteCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
