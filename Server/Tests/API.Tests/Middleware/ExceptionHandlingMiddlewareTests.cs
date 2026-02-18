using System.Text.Json;
using API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int statusCode, string body)> InvokeWithExceptionAsync(Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_ReturnsBadRequest()
    {
        var (statusCode, body) = await InvokeWithExceptionAsync(new ArgumentException("Invalid payload"));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(400, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Contains("Invalid argument:", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ReturnsNotFound()
    {
        var (statusCode, body) = await InvokeWithExceptionAsync(new KeyNotFoundException("Missing resource"));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(404, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Contains("Resource not found:", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var (statusCode, body) = await InvokeWithExceptionAsync(new UnauthorizedAccessException("No access"));

        Assert.Equal(StatusCodes.Status401Unauthorized, statusCode);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(401, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Contains("Unauthorized access:", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_ReturnsInternalServerError()
    {
        var (statusCode, body) = await InvokeWithExceptionAsync(new InvalidOperationException("Boom"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(500, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(
            "An internal server error occurred. Please try again later.",
            doc.RootElement.GetProperty("message").GetString()
        );
    }
}
