using KRTBanking.Application.Interfaces.Services;
using KRTBanking.Application.Services;
using KRTBanking.Infrastructure.Extensions;
using KRTBanking.Infrastructure.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.AddDynamoDb(builder.Configuration);
builder.Services.AddDynamoDbHealthChecks();

var app = builder.Build();

try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing database tables...");
    
    var databaseInitializer = app.Services.GetRequiredService<IDatabaseInitializer>();
    await databaseInitializer.InitializeAsync();
    
    logger.LogInformation("Database initialization completed successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Failed to initialize database tables. Application cannot start without proper database setup.");
    Environment.Exit(1);
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
