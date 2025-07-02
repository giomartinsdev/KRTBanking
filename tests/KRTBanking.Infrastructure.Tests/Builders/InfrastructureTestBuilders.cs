using Amazon.DynamoDBv2.Model;
using KRTBanking.Domain.Context.Customer.Entities;
using KRTBanking.Domain.Context.Customer.ValueObjects;
using KRTBanking.Infrastructure.Data.Configuration;
using KRTBanking.Infrastructure.Data.Models;
using Microsoft.Extensions.Options;

namespace KRTBanking.Infrastructure.Tests.Builders;

/// <summary>
/// Test data builders for infrastructure layer tests.
/// Provides factory methods to create test data with default values that can be customized.
/// </summary>
public static class InfrastructureTestBuilders
{
    /// <summary>
    /// Builder for creating Customer domain entities for testing.
    /// </summary>
    public class CustomerBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _documentNumber = "11144477735";
        private string _name = "John Doe";
        private string _email = "john.doe@email.com";
        private Account? _account;
        private List<LimitEntry> _limitEntries = new();
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _updatedAt = DateTime.UtcNow;
        private long _version = 1;
        private bool _isActive = true;

        public CustomerBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public CustomerBuilder WithDocumentNumber(string documentNumber)
        {
            _documentNumber = documentNumber;
            return this;
        }

        public CustomerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CustomerBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public CustomerBuilder WithAccount(Account account)
        {
            _account = account;
            return this;
        }

        public CustomerBuilder WithLimitEntries(params LimitEntry[] limitEntries)
        {
            _limitEntries = limitEntries.ToList();
            return this;
        }

        public CustomerBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        public CustomerBuilder WithUpdatedAt(DateTime updatedAt)
        {
            _updatedAt = updatedAt;
            return this;
        }

        public CustomerBuilder WithVersion(long version)
        {
            _version = version;
            return this;
        }

        public CustomerBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public Customer Build()
        {
            var documentNumber = new DocumentNumber(_documentNumber);
            var account = _account ?? AnAccount().Build();
            
            var customer = Customer.Create(
                documentNumber,
                _name,
                _email,
                account,
                1000.00m, // initial limit amount
                "Initial Limit"); // initial limit description

            // Add additional limit entries if any
            foreach (var limitEntry in _limitEntries)
            {
                customer.AdjustLimit(limitEntry.Amount, limitEntry.Description);
            }

            // Use reflection to set internal properties for testing
            var customerType = typeof(Customer);
            var idField = customerType.BaseType?.GetProperty("Id");
            idField?.SetValue(customer, _id);
            
            customerType.GetProperty("UpdatedAt")?.SetValue(customer, _updatedAt);
            customerType.GetProperty("Version")?.SetValue(customer, _version);
            
            if (!_isActive)
            {
                customer.Deactivate("Test deactivation");
            }

            return customer;
        }
    }

    /// <summary>
    /// Builder for creating Account value objects for testing.
    /// </summary>
    public class AccountBuilder
    {
        private string _number = "123456"; // Valid account number that fits in int
        private Agency _agency = Agency.Agency1;
        private decimal _balance = 1000.00m;

        public AccountBuilder WithNumber(string number)
        {
            _number = number;
            return this;
        }

        public AccountBuilder WithAgency(Agency agency)
        {
            _agency = agency;
            return this;
        }

        public AccountBuilder WithBalance(decimal balance)
        {
            _balance = balance;
            return this;
        }

        public Account Build()
        {
            return new Account(_agency, int.Parse(_number));
        }
    }

    /// <summary>
    /// Builder for creating LimitEntry value objects for testing.
    /// </summary>
    public class LimitEntryBuilder
    {
        private decimal _amount = 500.00m;
        private string _description = "Test Limit Entry";

        public LimitEntryBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public LimitEntryBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public LimitEntry Build()
        {
            return new LimitEntry(_amount, _description);
        }
    }

    /// <summary>
    /// Builder for creating DynamoDB attribute value dictionaries for testing.
    /// </summary>
    public class DynamoDbItemBuilder
    {
        private readonly Dictionary<string, AttributeValue> _attributes = new();

        public DynamoDbItemBuilder WithAttribute(string name, string value)
        {
            _attributes[name] = new AttributeValue { S = value };
            return this;
        }

        public DynamoDbItemBuilder WithAttribute(string name, bool value)
        {
            _attributes[name] = new AttributeValue { BOOL = value };
            return this;
        }

        public DynamoDbItemBuilder WithAttribute(string name, long value)
        {
            _attributes[name] = new AttributeValue { N = value.ToString() };
            return this;
        }

        public DynamoDbItemBuilder WithCustomerAttributes(Guid customerId, string documentNumber, string name, string email)
        {
            var pk = CustomerDynamoDbModel.CreatePartitionKey(customerId);
            var gsi1pk = CustomerDynamoDbModel.CreateGsi1PartitionKey(documentNumber);

            return WithAttribute("PK", pk)
                .WithAttribute("SK", "CUSTOMER")
                .WithAttribute("CustomerId", customerId.ToString())
                .WithAttribute("DocumentNumber", documentNumber)
                .WithAttribute("Name", name)
                .WithAttribute("Email", email)
                .WithAttribute("Account", """{"number": "0001-123456", "createdAt": "2024-01-01T00:00:00Z"}""")
                .WithAttribute("GSI1PK", gsi1pk)
                .WithAttribute("GSI1SK", "CUSTOMER")
                .WithAttribute("Limits", "[]")
                .WithAttribute("CreatedAt", DateTime.UtcNow.ToString("O"))
                .WithAttribute("UpdatedAt", DateTime.UtcNow.ToString("O"))
                .WithAttribute("Version", 1L)
                .WithAttribute("IsActive", true);
        }

        public Dictionary<string, AttributeValue> Build()
        {
            return new Dictionary<string, AttributeValue>(_attributes);
        }
    }

    /// <summary>
    /// Builder for creating DynamoDbOptions for testing.
    /// </summary>
    public class DynamoDbOptionsBuilder
    {
        private string _customerTableName = "TestCustomers";
        private string _region = "us-east-1";
        private bool _useLocalDb = true;

        public DynamoDbOptionsBuilder WithCustomerTableName(string tableName)
        {
            _customerTableName = tableName;
            return this;
        }

        public DynamoDbOptionsBuilder WithRegion(string region)
        {
            _region = region;
            return this;
        }

        public DynamoDbOptionsBuilder WithUseLocalDb(bool useLocalDb)
        {
            _useLocalDb = useLocalDb;
            return this;
        }

        public IOptions<DynamoDbOptions> Build()
        {
            var options = new DynamoDbOptions
            {
                CustomerTableName = _customerTableName,
                Region = _region,
                UseLocalDb = _useLocalDb
            };

            return Microsoft.Extensions.Options.Options.Create(options);
        }
    }

    // Static factory methods for common test scenarios
    public static CustomerBuilder ACustomer() => new();
    public static AccountBuilder AnAccount() => new();
    public static LimitEntryBuilder ALimitEntry() => new();
    public static DynamoDbItemBuilder ADynamoDbItem() => new();
    public static DynamoDbOptionsBuilder DynamoDbOptions() => new();

    // Pre-built common test data
    public static Customer DefaultCustomer() => ACustomer().Build();
    public static Account DefaultAccount() => AnAccount().Build();
    public static LimitEntry DefaultLimitEntry() => ALimitEntry().Build();
}
