using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KRTBanking.Application.Interfaces.Services;
using KRTBanking.Application.Services;
using KRTBanking.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddDynamoDb(builder.Configuration);

var app = builder.Build();

try
{
    var dynamoDbClient = app.Services.GetRequiredService<IAmazonDynamoDB>();
    await CreateTableIfNotExists(dynamoDbClient);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to initialize DynamoDB tables. The application will continue without table initialization.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static async Task CreateTableIfNotExists(IAmazonDynamoDB client)
{
    var listTable = new List<string>()
    {
        "KRTBanking-Customers",
    };

    foreach (var tableName in listTable)
    {
        try
        {
            var tableResponse = await client.DescribeTableAsync(tableName);
        }
        catch (ResourceNotFoundException)
        {
            var createTableRequest = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Id", KeyType.HASH)
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            await client.CreateTableAsync(createTableRequest);

            var tableStatus = "CREATING";
            while (tableStatus == "CREATING")
            {
                await Task.Delay(1000);
                var response = await client.DescribeTableAsync(tableName);
                tableStatus = response.Table.TableStatus;
            }
        }
    }
}