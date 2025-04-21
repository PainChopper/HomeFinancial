using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using HomeFinancial.Application.Common;
using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Infrastructure.Identity;

/// <summary>
/// Пользователь приложения, совместимый с ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser<long>
{
    public bool IsActive(IDateTimeProvider time) =>
        !LockoutEnabled || LockoutEnd == null || LockoutEnd <= time.UtcNow;

}
