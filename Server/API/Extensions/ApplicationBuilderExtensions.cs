using Infrastructure.Data;
using Core.Models;
using Core.Interfaces.Services;
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

        await SeedData.SeedAsync(context, roleManager, userManager, tokenService);
    }
}
