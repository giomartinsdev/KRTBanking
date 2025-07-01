using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KRTBanking.Infrastructure.HealthChecks;

/// <summary>
/// Health check for DynamoDB table readiness.
/// Validates that required tables exist and are accessible.
/// </summary>
public class DynamoDbHealthCheck : IHealthCheck
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string[] _requiredTables = { "KRTBanking-Customers" };

    public DynamoDbHealthCheck(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthData = new Dictionary<string, object>();
            
            foreach (var tableName in _requiredTables)
            {
                var response = await _dynamoDbClient.DescribeTableAsync(tableName, cancellationToken);
                var tableStatus = response.Table.TableStatus;
                
                healthData[tableName] = new
                {
                    Status = tableStatus,
                    ItemCount = response.Table.ItemCount,
                    TableSizeBytes = response.Table.TableSizeBytes
                };
                
                if (tableStatus != "ACTIVE")
                {
                    return HealthCheckResult.Unhealthy(
                        $"Table {tableName} is not active. Current status: {tableStatus}",
                        data: healthData);
                }
            }

            return HealthCheckResult.Healthy("All DynamoDB tables are active and accessible", healthData);
        }
        catch (ResourceNotFoundException ex)
        {
            return HealthCheckResult.Unhealthy($"Required DynamoDB table not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"DynamoDB health check failed: {ex.Message}");
        }
    }
}
