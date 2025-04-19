// Program.cs
using HomeFinancial.Infrastructure;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using HomeFinancial.Application;
using HomeFinancial.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog из файла конфигурации
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// Используем Serilog вместо стандартного логгера
builder.Host.UseSerilog();

// Infrastructure: DbContext, репозитории и др.
builder.Services.AddInfrastructure(builder.Configuration);

// Application-слой
builder.Services.AddApplicationServices();

// Swagger
builder.Services.AddSwaggerWithXmlDocs();

// Добавляем поддержку контроллеров
builder.Services.AddControllers();

var app = builder.Build();

// Глобальный обработчик ошибок
app.UseGlobalExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());

// Получаем логгер приложения
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Приложение запускается.");

// Проверка соединения с базой данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<HomeFinancialDbContext>();

    logger.LogInformation("Проверка соединения с базой данных.");

    if (dbContext.Database.CanConnect())
    {
        logger.LogInformation("Успешно подключено к базе данных.");
    }
    else
    {
        logger.LogError("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
        throw new InvalidOperationException("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
    }
}

// Включаем Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Включаем маршрутизацию контроллеров
app.MapControllers();

app.Run();
