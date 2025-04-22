using HomeFinancial.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace HomeFinancial.Infrastructure.Identity;

/// <summary>
/// Пользователь приложения, совместимый с ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser<long>
{
    public bool IsActive(IDateTimeProvider time) =>
        !LockoutEnabled || LockoutEnd == null || LockoutEnd <= time.UtcNow;

}
