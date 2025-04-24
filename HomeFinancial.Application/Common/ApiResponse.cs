namespace HomeFinancial.Application.Common;

public class ApiResponse<T>(bool success, T? data = default, string? errorMessage = null)
{
    public bool Success { get; } = success;
    public T? Data { get; } = data;
    public string? ErrorMessage { get; } = errorMessage;
}
