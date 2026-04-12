using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;

namespace API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<DataContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var tokenService = services.GetRequiredService<ITokenService>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
        var config = services.GetRequiredService<IConfiguration>();
        var env = services.GetRequiredService<IHostEnvironment>();

        var bootstrapSection = config.GetSection("Bootstrap");
        var bootstrapEnabled = bootstrapSection.GetValue<bool?>("Enabled") ?? env.IsDevelopment();
        var bootstrapTokenTtlHours = bootstrapSection.GetValue<int?>("TokenTtlHours") ?? 1;
        var logPlainTokenToPersistentLogs = bootstrapSection.GetValue<bool?>("LogPlainTokenToPersistentLogs") ?? env.IsDevelopment();
        var writePlainTokenToConsole = bootstrapSection.GetValue<bool?>("WritePlainTokenToConsole") ?? !env.IsDevelopment();

        await SeedData.SeedAsync(
            context,
            roleManager,
            userManager,
            tokenService,
            logger,
            bootstrapEnabled,
            bootstrapTokenTtlHours,
            logPlainTokenToPersistentLogs,
            writePlainTokenToConsole
        );
    }

    public static IApplicationBuilder UseConfiguredSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
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
    }

    public static IApplicationBuilder UseApiCsrfProtection(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
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
    }

    public static IApplicationBuilder UseSpaStaticFilesWithCaching(this IApplicationBuilder app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context => ApplySpaCacheHeaders(context.Context)
        });

        return app;
    }

    public static IEndpointRouteBuilder MapSpaFallbackWithCaching(this IEndpointRouteBuilder app, IHostEnvironment environment)
    {
        var webRootPath = environment.ContentRootPath;
        if (environment is IWebHostEnvironment webEnv)
        {
            webRootPath = webEnv.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var spaIndexPath = Path.Combine(webRootPath, "index.html");
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

        return app;
    }

    private static bool IsStaticFile(string path)
    {
        var staticExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".css", ".js", ".woff", ".woff2", ".ttf", ".ico" };
        return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static void ApplySpaCacheHeaders(HttpContext context)
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
            return;
        }

        if (IsEntityContent(path))
        {
            // Entity assets keep stable URLs (e.g. /saints/{slug}/image.webp),
            // so force revalidation to avoid stale images after in-place updates.
            headers.CacheControl = "public, max-age=0, must-revalidate";
        }
    }

    private static bool IsHtmlRequest(string path)
    {
        return string.Equals(path, "/", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHashedSpaAsset(string path)
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

    private static bool IsEntityContent(string path)
    {
        return path.StartsWith("/saints/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/miracles/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/prayers/", StringComparison.OrdinalIgnoreCase);
    }
}
