using System.Text.Json;
using System.Threading.RateLimiting;
using API.Helpers;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    // ---------------- Infrastructure ----------------

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithOrigins("http://localhost:4200")
                      .AllowCredentials();
            });
        });

        return services;
    }


    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 120,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationCookieConfig(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.Name = "AuthCookie";

            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };

            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        return services;
    }

    // ---------------- Application ----------------

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ISaintsRepository, SaintsRepository>();
        services.AddScoped<IMiraclesRepository, MiraclesRepository>();
        services.AddScoped<IPrayersRepository, PrayersRepository>();
        services.AddScoped<IReligiousOrdersRepository, ReligiousOrdersRepository>();
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<IRecentActivityRepository, RecentActivityRepository>();

        // Services
        services.AddScoped<ISaintsService, SaintsService>();
        services.AddScoped<IMiraclesService, MiraclesService>();
        services.AddScoped<IPrayersService, PrayersService>();
        services.AddScoped<ITagsService, TagsService>();
        services.AddScoped<IReligiousOrdersService, ReligiousOrdersService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountTokensService, AccountTokensService>();
        services.AddSingleton<IEmailSender<AppUser>, EmailSender>();
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    public static IServiceCollection AddMemoryCacheWithLimit(this IServiceCollection services, int sizeLimit = 60_000)
    {
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = sizeLimit;
        });

        return services;
    }

    // ---------------- Database ----------------

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("defaultConnection");

        services.AddDbContext<DataContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 1, 0))
            )
        );

        return services;
    }

    // ---------------- Identity ----------------

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;

            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        })
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}