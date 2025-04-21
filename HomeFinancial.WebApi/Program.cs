using HomeFinancial.Infrastructure;
using Serilog;
using HomeFinancial.Application;
using HomeFinancial.WebApi;

using HomeFinancial.WebApi.Auth;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Регистрация политики доступа только к своим данным
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnData", policy =>
        policy.Requirements.Add(new OwnDataRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, OwnDataHandler>();
builder.Services.AddHttpContextAccessor();

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
