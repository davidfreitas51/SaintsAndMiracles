using API.Extensions;
using API.Middleware;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

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

app.UseConfiguredSerilogRequestLogging();

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

app.UseApiCsrfProtection();
app.UseSpaStaticFilesWithCaching();

app.MapGet("/api/security/csrf-token", (HttpContext context, IAntiforgery antiforgery) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});

app.MapControllers();
app.MapSpaFallbackWithCaching(app.Environment);

app.Run();
