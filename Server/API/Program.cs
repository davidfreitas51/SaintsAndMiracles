using API.Extensions;
using API.Middleware;
using Serilog;
using Serilog.Events;

// Bootstrap logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
// Serilog via appsettings.json
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
); ;

// Controllers + JSON
builder.Services.AddApiControllers();

// Infrastructure
builder.Services.AddCorsPolicy();
builder.Services.AddRateLimiting();
builder.Services.AddApplicationCookieConfig();

builder.Services.AddMemoryCacheWithLimit();
builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();

var app = builder.Build();

// Serilog HTTP request logging with smart filtering
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;

        // Skip logging for static files (images, styles, scripts, fonts)
        if (IsStaticFile(path))
            return LogEventLevel.Debug;

        // Skip logging for CORS preflight requests
        if (method == "OPTIONS")
            return LogEventLevel.Debug;

        // Log errors appropriately
        if (ex is not null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        // Log mutations at Information, quiet routine reads
        if (method != "GET")
            return LogEventLevel.Information;

        // Flag slow reads for visibility
        if (elapsed > 500)
            return LogEventLevel.Warning;

        return LogEventLevel.Debug;
    };

    options.IncludeQueryInRequestPath = true;
});

// Seed
await app.SeedDatabaseAsync();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

app.Run();

/// <summary>
/// Checks if the request path is for a static file
/// </summary>
static bool IsStaticFile(string path)
{
    var staticExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".css", ".js", ".woff", ".woff2", ".ttf", ".ico" };
    return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
}