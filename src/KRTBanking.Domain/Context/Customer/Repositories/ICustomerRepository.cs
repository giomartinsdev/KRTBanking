using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.ValueObjects;

namespace KRTBanking.Domain.Context.Customer.Repositories;

/// <summary>
/// Repository interface for customer aggregate operations.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Gets a customer by their unique identifier.
    /// </summary>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Entities.Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a customer by their document number string.
    /// </summary>
    /// <param name="documentNumber">The customer's document number as string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Entities.Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Entities.Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer in the repository.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Entities.Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a customer from the repository.
    /// </summary>
    /// <param name="customer">The customer to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(Entities.Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all customers with pagination support.
    /// </summary>
    /// <param name="pageSize">The number of customers per page.</param>
    /// <param name="lastEvaluatedKey">The last evaluated key for pagination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of customers and the next pagination key.</returns>
    Task<(IEnumerable<Entities.Customer> customers, string? nextPageKey)> GetAllAsync(
        int pageSize = 50, 
        string? lastEvaluatedKey = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer with the specified document number exists.
    /// </summary>
    /// <param name="documentNumber">The document number to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if a customer with the document number exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(DocumentNumber documentNumber, CancellationToken cancellationToken = default);
}
