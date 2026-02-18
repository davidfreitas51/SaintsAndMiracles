using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

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
}
