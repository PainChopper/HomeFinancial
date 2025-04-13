// Program.cs
using HomeFinancial.Repository;
using HomeFinancial.DTOs;
using HomeFinancial.Mapping;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

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

// Grouped Routes for Categories
var categoryGroup = app.MapGroup("/categories");
categoryGroup.MapGet("/", async (ICategoryRepository categoryRepo, AppMapper mapper) =>
{
    var categories = await categoryRepo.GetAllAsync();
    var categoryDtos = categories.Select(c => mapper.CategoryToDto(c));
    return Results.Ok(categoryDtos);
});
categoryGroup.MapGet("/{id:int}", async (ICategoryRepository categoryRepo, AppMapper mapper, int id) =>
{
    var category = await categoryRepo.GetByIdAsync(id);
    return category != null ? Results.Ok(mapper.CategoryToDto(category)) : Results.NotFound();
});
categoryGroup.MapPost("/", async (ICategoryRepository categoryRepo, AppMapper mapper, CreateCategoryDto createDto) =>
{
    var category = mapper.DtoToCategory(createDto);
    try
    {
        var createdCategory = await categoryRepo.CreateAsync(category);
        var categoryDto = mapper.CategoryToDto(createdCategory);
        return Results.Created($"/categories/{createdCategory.Id}", categoryDto);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

// Grouped Routes for Transactions
var transactionGroup = app.MapGroup("/transactions");
transactionGroup.MapGet("/", async (ITransactionRepository transactionRepo, AppMapper mapper) =>
{
    var transactions = await transactionRepo.GetAllAsync();
    var transactionDtos = transactions.Select(tx => mapper.BankTransactionToDto(tx));
    return Results.Ok(transactionDtos);
});
transactionGroup.MapGet("/{id:int}", async (ITransactionRepository transactionRepo, AppMapper mapper, int id) =>
{
    var transaction = await transactionRepo.GetByIdAsync(id);
    return transaction != null ? Results.Ok(mapper.BankTransactionToDto(transaction)) : Results.NotFound();
});
transactionGroup.MapPost("/", async (ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, AppMapper mapper, CreateBankTransactionDto createDto) =>
{
    var category = await categoryRepo.GetOrCreateAsync(createDto.CategoryName);
    var transaction = mapper.DtoToBankTransaction(createDto, category.Id);

    try
    {
        var createdTransaction = await transactionRepo.CreateAsync(transaction);
        var transactionDto = mapper.BankTransactionToDto(createdTransaction);
        return Results.Created($"/transactions/{createdTransaction.Id}", transactionDto);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

// Grouped Routes for Files
var fileGroup = app.MapGroup("/files");
fileGroup.MapGet("/exist/{fileName}", async (IFileRepository fileRepo, string fileName) =>
{
    var exists = await fileRepo.ExistsAsync(fileName);
    return Results.Ok(exists);
});
fileGroup.MapGet("/", async (IFileRepository fileRepo, AppMapper mapper) =>
{
    var files = await fileRepo.GetAllAsync();
    var fileDtos = files.Select(f => mapper.ImportedFileToDto(f));
    return Results.Ok(fileDtos);
});
fileGroup.MapGet("/{id:int}", async (IFileRepository fileRepo, AppMapper mapper, int id) =>
{
    var file = await fileRepo.GetByIdAsync(id);
    return file != null ? Results.Ok(mapper.ImportedFileToDto(file)) : Results.NotFound();
});
fileGroup.MapPost("/", async (IFileRepository fileRepo, AppMapper mapper, CreateImportedFileDto createDto) =>
{
    var file = mapper.DtoToImportedFile(createDto);
    try
    {
        var createdFile = await fileRepo.CreateAsync(file);
        var fileDto = mapper.ImportedFileToDto(createdFile);
        return Results.Created($"/files/{createdFile.Id}", fileDto);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});

app.Run();
