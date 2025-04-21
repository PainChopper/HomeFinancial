using Microsoft.AspNetCore.Authorization;

namespace HomeFinancial.WebApi.Auth;

/// <summary>
/// Требование для проверки доступа только к своим данным.
/// </summary>
public class OwnDataRequirement : IAuthorizationRequirement { }
