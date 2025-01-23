// Program.cs
using HomeFinancial.Repository;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using HomeFinancial.Data.Models;

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

// Apply migrations and check database connection
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

// Grouped Routes for Categories
var categoryGroup = app.MapGroup("/categories");
categoryGroup.MapGet("/", async (ICategoryRepository categoryRepo) => Results.Ok((object?)await categoryRepo.GetAllAsync()));
categoryGroup.MapGet("/{id:int}", async (ICategoryRepository categoryRepo, int id) =>
{
    var category = await categoryRepo.GetByIdAsync(id);
    return category != null ? Results.Ok(category) : Results.NotFound();
});
categoryGroup.MapPost("/", async (ICategoryRepository categoryRepo, Category category) =>
{
    try
    {
        var createdCategory = await categoryRepo.CreateAsync(category);
        return Results.Created($"/categories/{createdCategory.Id}", createdCategory);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

// Grouped Routes for Transactions
var transactionGroup = app.MapGroup("/transactions");
transactionGroup.MapGet("/", async (ITransactionRepository transactionRepo) => Results.Ok((object?)await transactionRepo.GetAllAsync()));
transactionGroup.MapGet("/{id:int}", async (ITransactionRepository transactionRepo, int id) =>
{
    var transaction = await transactionRepo.GetByIdAsync(id);
    return transaction != null ? Results.Ok(transaction) : Results.NotFound();
});
transactionGroup.MapPost("/", async (ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, BankTransaction tx) =>
{
    var category = await categoryRepo.GetOrCreateAsync(tx.Category.Name);

    tx.CategoryId = category.Id;

    try
    {
        var createdTransaction = await transactionRepo.CreateAsync(tx);
        return Results.Created($"/transactions/{createdTransaction.Id}", createdTransaction);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

// Grouped Routes for Files
var fileGroup = app.MapGroup("/files");
fileGroup.MapGet("/exist/{fileName}", async (IFileRepository fileRepo, string fileName) => Results.Ok((object?)await fileRepo.ExistsAsync(fileName)));
fileGroup.MapGet("/", async (IFileRepository fileRepo) => Results.Ok((object?)await fileRepo.GetAllAsync()));
fileGroup.MapGet("/{id:int}", async (IFileRepository fileRepo, int id) =>
{
    var file = await fileRepo.GetByIdAsync(id);
    return file != null ? Results.Ok(file) : Results.NotFound();
});
fileGroup.MapPost("/", async (IFileRepository fileRepo, ImportedFile file) =>
{
    try
    {
        var createdFile = await fileRepo.CreateAsync(file);
        return Results.Created($"/files/{createdFile.Id}", createdFile);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.Run();
