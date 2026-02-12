using API.Extensions;
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
builder.Services.AddCorsPolicy();
builder.Services.AddRateLimiting();
builder.Services.AddApplicationCookieConfig();

builder.Services.AddMemoryCacheWithLimit();
builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();

var app = builder.Build();

// Serilog HTTP request logging
app.UseSerilogRequestLogging();

// Seed
await app.SeedDatabaseAsync();

// Middleware pipeline
app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

app.Run();