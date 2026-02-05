using System.Text.Json;
using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services.AddApiControllers();

// Infrastructure
builder.Services.AddCorsPolicy();
builder.Services.AddRateLimiting();
builder.Services.AddApplicationCookieConfig();

builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();

var app = builder.Build();

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