using HomeFinancial.Infrastructure;
using Serilog;
using HomeFinancial.Application;
using HomeFinancial.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog из файла конфигурации
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// Используем Serilog вместо стандартного логгера
builder.Host.UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services
            .AddInfrastructure(context.Configuration)
            .AddApplicationServices()
            .AddSwaggerWithXmlDocs()
            .AddControllers();
    });

var app = builder.Build();

// Получаем логгер приложения
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Глобальный обработчик ошибок
app.UseGlobalExceptionHandler(logger);

// Проверка соединения с базой данных
app.Services.CheckDatabaseConnection(logger);

// Включаем Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Включаем маршрутизацию контроллеров
app.MapControllers();

app.Run();
