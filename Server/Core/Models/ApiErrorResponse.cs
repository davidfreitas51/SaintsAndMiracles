namespace Core.Models;

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string>? Details { get; set; }
}

