using System.Text.Json;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Models;

namespace KRTBanking.Infrastructure.Data.Mappers;

/// <summary>
/// Maps between domain entities and DynamoDB models.
/// </summary>
public static class CustomerMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts a domain Customer entity to a DynamoDB model.
    /// </summary>
    /// <param name="customer">The domain customer entity.</param>
    /// <returns>The DynamoDB model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when customer is null.</exception>
    public static CustomerDynamoDbModel ToModel(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var accountJson = customer.Account is not null
            ? JsonSerializer.Serialize(new AccountModel
            {
                Number = customer.Account.Number,
                CreatedAt = customer.Account.CreatedAt
            }, JsonOptions)
            : string.Empty;

        var limitEntriesJson = customer.LimitEntries.Count > 0
            ? JsonSerializer.Serialize(customer.LimitEntries.Select(entry => new LimitEntryModel
            {
                Amount = entry.Amount,
                Description = entry.Description,
                CreatedAt = entry.CreatedAt
            }).ToList(), JsonOptions)
            : string.Empty;

        return new CustomerDynamoDbModel
        {
            PK = CustomerDynamoDbModel.CreatePartitionKey(customer.Id),
            SK = "CUSTOMER",
            CustomerId = customer.Id.ToString(),
            DocumentNumber = customer.DocumentNumber.Value,
            Name = customer.Name,
            Email = customer.Email,
            Account = accountJson,
            Limits = limitEntriesJson,
            CreatedAt = customer.CreatedAt.ToString("O"),
            UpdatedAt = customer.UpdatedAt.ToString("O"),
            Version = customer.Version,
            IsActive = customer.IsActive,
            GSI1PK = CustomerDynamoDbModel.CreateGsi1PartitionKey(customer.DocumentNumber.Value),
            GSI1SK = "CUSTOMER"
        };
    }

    /// <summary>
    /// Converts a DynamoDB model to a domain Customer entity.
    /// </summary>
    /// <param name="model">The DynamoDB model.</param>
    /// <returns>The domain customer entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required data is invalid.</exception>
    public static Customer ToDomain(CustomerDynamoDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var documentNumber = DocumentNumber.TryCreate(model.DocumentNumber)
            ?? throw new InvalidOperationException($"Invalid document number: {model.DocumentNumber}");

        if (!Guid.TryParse(model.CustomerId, out var customerId))
        {
            throw new InvalidOperationException($"Invalid customer ID: {model.CustomerId}");
        }

        if (!DateTime.TryParse(model.CreatedAt, out var createdAt))
        {
            throw new InvalidOperationException($"Invalid created date: {model.CreatedAt}");
        }

        if (!DateTime.TryParse(model.UpdatedAt, out var updatedAt))
        {
            throw new InvalidOperationException($"Invalid updated date: {model.UpdatedAt}");
        }

        Account? account = null;
        if (!string.IsNullOrEmpty(model.Account))
        {
            try
            {
                var accountModel = JsonSerializer.Deserialize<AccountModel>(model.Account, JsonOptions);
                if (accountModel is not null)
                {
                    var accountParts = accountModel.Number.Split('-');
                    if (accountParts.Length == 2 && 
                        int.TryParse(accountParts[0], out var agencyCode) && 
                        int.TryParse(accountParts[1], out var accountNumber))
                    {
                        var agency = (Agency)agencyCode;
                        account = Account.Reconstruct(agency, accountNumber, accountModel.CreatedAt);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid account number format: {accountModel.Number}");
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid account data: {model.Account}", ex);
            }
        }

        var limitEntries = new List<LimitEntry>();
        if (!string.IsNullOrEmpty(model.Limits))
        {
            try
            {
                var limitEntryModels = JsonSerializer.Deserialize<List<LimitEntryModel>>(model.Limits, JsonOptions);
                if (limitEntryModels is not null)
                {
                    limitEntries.AddRange(limitEntryModels.Select(entry => 
                        LimitEntry.Reconstruct(entry.Amount, entry.Description, entry.CreatedAt)));
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid limit entries data: {model.Limits}", ex);
            }
        }

        return Customer.Reconstruct(
            id: customerId,
            documentNumber: documentNumber,
            name: model.Name,
            email: model.Email,
            account: account ?? throw new InvalidOperationException("Account data is required for customer reconstruction"),
            limitEntries: limitEntries,
            isActive: model.IsActive,
            createdAt: createdAt,
            updatedAt: updatedAt,
            version: model.Version
        );
    }
}
