using HomeFinancial.Application;
using HomeFinancial.Infrastructure;
using HomeFinancial.WebApi;
using HomeFinancial.WebApi.Auth;
using HomeFinancial.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Serilog;

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
            .AddApplicationServices(context.Configuration)
            .AddSwaggerWithXmlDocs()
            .AddControllers()
            .ConfigureCustomApiBehavior();
    });

var app = builder.Build();

// Глобальный обработчик ошибок
app.UseGlobalExceptionHandler();

// Проверка соединения с базой данных
if (!await app.Services.CheckDatabaseConnectionAsync())
{
    Environment.Exit(1);
}

// Автоматическое применение миграций EF Core в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.ApplyDatabaseMigrations();
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
