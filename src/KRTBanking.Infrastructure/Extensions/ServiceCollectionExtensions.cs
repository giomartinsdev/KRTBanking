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
            return new DynamoDBContext(client);
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
}
