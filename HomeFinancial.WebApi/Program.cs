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

// Глобальный обработчик ошибок
app.UseGlobalExceptionHandler();

// Проверка соединения с базой данных
if (!app.Services.CheckDatabaseConnection())
{
    Environment.Exit(1);
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
