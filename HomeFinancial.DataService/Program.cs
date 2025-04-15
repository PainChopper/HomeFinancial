// Program.cs
using HomeFinancial.Repository;
using HomeFinancial.Mapping;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using HomeFinancial.Data;
using HomeFinancial.DataService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from configuration file
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// Use Serilog instead of the default logger
builder.Host.UseSerilog();

// Get the connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// Validate the connection string
if (string.IsNullOrWhiteSpace(connectionString))
{
    Log.Logger.Error("Connection string '{ConnectionName}' is not defined in the configuration.", nameof(connectionString));
    throw new InvalidOperationException($"Connection string '{nameof(connectionString)}' is missing.");
}

// Register DbContext
builder.Services.AddDbContext<HomeFinancialDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Register repositories using extension method
builder.Services.AddRepositories();

// Register Mapperly generated mapper
builder.Services.AddSingleton<AppMapper>();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global Exception Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            Log.Logger.Error(contextFeature.Error, "An unhandled exception occurred.");
            var errorResponse = new
            {
                context.Response.StatusCode,
                Message = "Internal Server Error."
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    });
});

// Get the logger once and store it in a variable
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting the application.");

// Check database connection
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<HomeFinancialDbContext>();

    logger.LogInformation("Checking database connection.");

    if (dbContext.Database.CanConnect())
    {
        logger.LogInformation("Successfully connected to the database.");
    }
    else
    {
        logger.LogError("Failed to connect to the database.");
        throw new Exception("Cannot establish a connection to the database.");
    }
}

// Enable Swagger in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map endpoints from separate files
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
app.MapFileEndpoints();

app.Run();
