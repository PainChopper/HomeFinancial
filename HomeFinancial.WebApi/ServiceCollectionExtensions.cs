using System.Reflection;

namespace HomeFinancial.WebApi;

/// <summary>
/// Методы расширения для сервисов WebApi (Swagger и др.)
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует Swagger с поддержкой xml-комментариев контроллеров
    /// </summary>
    public static IServiceCollection AddSwaggerWithXmlDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; // Имя XML файла документации
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });
        return services;
    }
}
