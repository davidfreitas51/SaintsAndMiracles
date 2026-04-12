using API.Extensions;
using API.Middleware;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.HttpOverrides;
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
builder.Services.AddCorsPolicy(builder.Environment);
builder.Services.AddRateLimiting();
builder.Services.AddApplicationCookieConfig();
builder.Services.AddCsrfProtection(builder.Environment);
builder.Services.AddHsts(options =>
{
    options.Preload = false;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(180);
});

builder.Services.AddMemoryCacheWithLimit();
builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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

    // Avoid persisting sensitive query-string tokens (email confirmation/reset links) in logs.
    options.IncludeQueryInRequestPath = false;
});

// Seed
await app.SeedDatabaseAsync();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Configuration.GetValue("Security:UseHttpsRedirection", !app.Environment.IsDevelopment()))
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path;

    var isSafeMethod = HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);
    var isApiRequest = path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    var isCsrfTokenEndpoint = path.Equals("/api/security/csrf-token", StringComparison.OrdinalIgnoreCase);

    if (isApiRequest && !isSafeMethod && !isCsrfTokenEndpoint)
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        await antiforgery.ValidateRequestAsync(context);
    }

    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context => ApplySpaCacheHeaders(context.Context)
});

app.MapGet("/api/security/csrf-token", (HttpContext context, IAntiforgery antiforgery) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});

app.MapControllers();

var spaIndexPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "index.html");
if (File.Exists(spaIndexPath))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        OnPrepareResponse = context => ApplySpaCacheHeaders(context.Context)
    });
}
else
{
    app.MapFallback(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("SPA entry point not found. Build the Angular app into wwwroot.");
    });
}

app.Run();

/// <summary>
/// Checks if the request path is for a static file
/// </summary>
static bool IsStaticFile(string path)
{
    var staticExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".css", ".js", ".woff", ".woff2", ".ttf", ".ico" };
    return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
}

static void ApplySpaCacheHeaders(HttpContext context)
{
    var path = context.Request.Path.Value ?? string.Empty;
    var headers = context.Response.Headers;

    if (IsHtmlRequest(path))
    {
        // Always re-fetch HTML so clients get the latest hashed bundle references after deploy.
        headers.CacheControl = "no-cache, no-store, must-revalidate";
        headers.Pragma = "no-cache";
        headers.Expires = "0";
        return;
    }

    if (IsHashedSpaAsset(path))
    {
        headers.CacheControl = "public, max-age=31536000, immutable";
    }
}

static bool IsHtmlRequest(string path)
{
    return string.Equals(path, "/", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
}

static bool IsHashedSpaAsset(string path)
{
    var fileName = Path.GetFileName(path);
    if (string.IsNullOrWhiteSpace(fileName))
    {
        return false;
    }

    var isJsOrCss = fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
    if (!isJsOrCss)
    {
        return false;
    }

    var dashIndex = fileName.LastIndexOf('-');
    var dotIndex = fileName.LastIndexOf('.');
    if (dashIndex < 0 || dotIndex <= dashIndex + 1)
    {
        return false;
    }

    var hashPart = fileName[(dashIndex + 1)..dotIndex];
    return hashPart.Length >= 8 && hashPart.All(char.IsLetterOrDigit);
}