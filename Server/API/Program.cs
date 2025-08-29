using System.Text.Json;
using API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:4200")
              .AllowCredentials();
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Name = "AuthCookie";

    options.LoginPath = "/api/accounts/login";
    options.LogoutPath = "/api/accounts/logout";

    options.ExpireTimeSpan = TimeSpan.FromDays(2);
    options.SlidingExpiration = true;

    options.Events.OnValidatePrincipal = async context =>
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);

        var issuedUtc = context.Properties.IssuedUtc;
        if (issuedUtc.HasValue && issuedUtc.Value.AddDays(14) < DateTimeOffset.UtcNow)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    };
});

var app = builder.Build();

// Seed
await app.SeedDatabaseAsync();

// Middlewares
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
