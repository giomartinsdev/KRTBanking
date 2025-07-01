using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.Repositories;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Models;

namespace KRTBanking.Infrastructure.Data.Repositories;

/// <summary>
/// DynamoDB implementation of the customer repository using the article pattern.
/// </summary>
public sealed class CustomerDynamoRepository : ICustomerRepository
{
    private readonly IDynamoDBContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerDynamoRepository"/> class.
    /// </summary>
    /// <param name="context">The DynamoDB context.</param>
    public CustomerDynamoRepository(IDynamoDBContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    /// <param name="customer">The customer to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var model = MapToModel(customer);
        await _context.SaveAsync(model, cancellationToken);
    }

    /// <summary>
    /// Updates an existing customer in the repository.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var model = MapToModel(customer);
        await _context.SaveAsync(model, cancellationToken);
    }

    /// <summary>
    /// Deletes a customer from the repository.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _context.DeleteAsync<CustomerDynamoModel>(id.ToString(), cancellationToken);
    }

    /// <summary>
    /// Gets a customer by identifier.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.LoadAsync<CustomerDynamoModel>(id.ToString(), cancellationToken);
        return model == null ? null : MapToDomain(model);
    }

    /// <summary>
    /// Gets a customer by document number.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<Customer?> GetByDocumentNumberAsync(DocumentNumber documentNumber, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documentNumber);

        var search = _context.ScanAsync<CustomerDynamoModel>(new List<ScanCondition>
        {
            new("DocumentNumber", ScanOperator.Equal, documentNumber.Value)
        });

        var results = await search.GetRemainingAsync(cancellationToken);
        var model = results.FirstOrDefault();
        
        return model == null ? null : MapToDomain(model);
    }

    /// <summary>
    /// Gets a customer by document number string.
    /// </summary>
    /// <param name="documentNumber">The document number as string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

        var search = _context.ScanAsync<CustomerDynamoModel>(new List<ScanCondition>
        {
            new("DocumentNumber", ScanOperator.Equal, documentNumber)
        });

        var results = await search.GetRemainingAsync(cancellationToken);
        var model = results.FirstOrDefault();
        
        return model == null ? null : MapToDomain(model);
    }

    /// <summary>
    /// Removes a customer from the repository.
    /// </summary>
    /// <param name="customer">The customer to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RemoveAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await DeleteAsync(customer.Id, cancellationToken);
    }

    /// <summary>
    /// Gets all customers with pagination.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <param name="lastEvaluatedKey">The last evaluated key for pagination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the customers and the next page key.</returns>
    public async Task<(IEnumerable<Customer> customers, string? nextPageKey)> GetAllAsync(
        int pageSize = 50, 
        string? lastEvaluatedKey = null, 
        CancellationToken cancellationToken = default)
    {
        var search = _context.ScanAsync<CustomerDynamoModel>(new List<ScanCondition>());
        var results = await search.GetRemainingAsync(cancellationToken);
        
        var customers = results.Select(MapToDomain);
        
        // For simplicity, return all results without pagination in this implementation
        return (customers, null);
    }

    /// <summary>
    /// Gets customers by account number prefix.
    /// </summary>
    /// <param name="accountNumberPrefix">The account number prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching customers.</returns>
    public async Task<IEnumerable<Customer>> GetByAccountNumberPrefixAsync(string accountNumberPrefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumberPrefix);

        var search = _context.ScanAsync<CustomerDynamoModel>(new List<ScanCondition>
        {
            new("AccountNumber", ScanOperator.BeginsWith, accountNumberPrefix)
        });

        var results = await search.GetRemainingAsync(cancellationToken);
        return results.Select(MapToDomain);
    }

    private static CustomerDynamoModel MapToModel(Customer customer)
    {
        return new CustomerDynamoModel
        {
            Id = customer.Id.ToString(),
            DocumentNumber = customer.DocumentNumber.Value,
            Name = customer.Name,
            Email = customer.Email,
            AccountNumber = customer.Account.Number,
            LimitAmount = customer.CurrentLimit,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Version = customer.Version
        };
    }

    private static Customer MapToDomain(CustomerDynamoModel model)
    {
        var documentNumber = new DocumentNumber(model.DocumentNumber);
        
        var accountParts = model.AccountNumber.Split('-');
        var agency = (Agency)int.Parse(accountParts[0]);
        var accountNumber = int.Parse(accountParts[1]);
        
        var account = Account.Create(agency, accountNumber);
        
        var limitEntries = new List<LimitEntry>();
        if (model.LimitAmount > 0)
        {
            limitEntries.Add(new LimitEntry(model.LimitAmount, "Initial Credit Limit"));
        }

        return Customer.Reconstruct(
            Guid.Parse(model.Id),
            documentNumber,
            model.Name,
            model.Email,
            account,
            limitEntries,
            model.CreatedAt,
            model.UpdatedAt,
            model.Version);
    }
}
