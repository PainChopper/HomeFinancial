using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void SetAllPropertiesRequired<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        foreach (var property in builder.Metadata.GetProperties())
        {
            // Пропускаем навигационные свойства и ключи
            if (!property.IsPrimaryKey() && !property.IsForeignKey())
            {
                property.IsNullable = false;
            }
        }
    }
}