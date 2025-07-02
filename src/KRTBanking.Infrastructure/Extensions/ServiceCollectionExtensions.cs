using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using KRTBanking.Domain.Context.Customer.Repositories;
using KRTBanking.Infrastructure.Data;
using KRTBanking.Infrastructure.Data.Repositories;
using KRTBanking.Infrastructure.HealthChecks;
using KRTBanking.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KRTBanking.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring DynamoDB services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DynamoDB services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDynamoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var awsConfigSection = configuration.GetSection("AwsConfig");
        var serviceUrl = awsConfigSection["ServiceURL"];
        var accessKey = awsConfigSection["AccessKey"];
        var secretKey = awsConfigSection["SecretKey"];

        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = serviceUrl
        };

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var amazonDynamoDbClient = new AmazonDynamoDBClient(credentials, config);
        
        services.AddSingleton<IAmazonDynamoDB>(amazonDynamoDbClient);

        services.AddSingleton<IDynamoDBContext>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
            return new DynamoDBContextBuilder()
                .WithDynamoDBClient(() => client)
                .Build();
        });

        services.AddScoped<ICustomerRepository, CustomerDynamoRepository>();

        services.AddTransient<IDatabaseInitializer, DynamoDbInitializer>();

        return services;
    }

    /// <summary>
    /// Adds DynamoDB health checks to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDynamoDbHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DynamoDbHealthCheck>("dynamodb");

        return services;
    }

    /// <summary>
    /// Adds all infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DynamoDB services
        services.AddDynamoDb(configuration);
        
        // Add health checks
        services.AddDynamoDbHealthChecks();

        return services;
    }

    /// <summary>
    /// Initializes the infrastructure services (database, etc.).
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A task representing the initialization operation.</returns>
    public static async Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            logger.LogInformation("Initializing infrastructure services...");
            
            var databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync();
            
            logger.LogInformation("Infrastructure initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to initialize infrastructure services. Application cannot start without proper infrastructure setup.");
            throw;
        }
    }
}
