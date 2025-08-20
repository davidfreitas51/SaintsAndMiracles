using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    private static string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/SeedData");
    private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public static async Task SeedAsync(
        DataContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager,
        ITokenService tokenService)
    {
        await context.Database.MigrateAsync();


        await SeedRoles(roleManager);
        await SeedBootstrapToken(context, userManager, tokenService);
        await SeedTags(context);
        await SeedSaints(context);
        await SeedMiracles(context);
        await SeedPrayers(context);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Employee", "Admin" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                IdentityRole role = new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedBootstrapToken(
        DataContext context,
        UserManager<AppUser> userManager,
        ITokenService tokenService)
    {
        var hasAdmin = (await userManager.GetUsersInRoleAsync("Admin")).Any();
        if (hasAdmin) return;

        var clearToken = tokenService.GenerateClearToken();
        var hash = tokenService.HashTokenBase64(clearToken);

        var bootstrapToken = new AccountToken
        {
            Hash = hash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
            Purpose = "InitialAdminBootstrap",
            IssuedTo = "System"
        };

        context.AccountTokens.Add(bootstrapToken);
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=================================================");
        Console.WriteLine(" NO ADMIN USERS FOUND ");
        Console.WriteLine(" Use this bootstrap token to create the first admin account:");
        Console.WriteLine();
        Console.WriteLine(clearToken);
        Console.WriteLine();
        Console.WriteLine($" This token will expire at: {bootstrapToken.ExpiresAtUtc}");
        Console.WriteLine("=================================================");
        Console.ResetColor();
    }

    private static async Task SeedTags(DataContext context)
    {
        if (!context.Tags.Any())
        {
            var filePath = Path.Combine(basePath, "tags.json");
            var json = await File.ReadAllTextAsync(filePath);

            var tags = JsonSerializer.Deserialize<List<Tag>>(json, jsonOptions);

            if (tags != null)
            {
                context.Tags.AddRange(tags);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedSaints(DataContext context)
    {
        if (!context.Saints.Any())
        {
            var filePath = Path.Combine(basePath, "saints.json");
            var json = await File.ReadAllTextAsync(filePath);
            var saints = JsonSerializer.Deserialize<List<Saint>>(json, jsonOptions);

            if (saints != null)
            {
                context.Saints.AddRange(saints);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedMiracles(DataContext context)
    {
        if (!context.Miracles.Any())
        {
            var filePath = Path.Combine(basePath, "miracles.json");
            var json = await File.ReadAllTextAsync(filePath);
            var miracles = JsonSerializer.Deserialize<List<Miracle>>(json, jsonOptions);

            if (miracles != null)
            {
                context.Miracles.AddRange(miracles);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedPrayers(DataContext context)
    {
        if (!context.Prayers.Any())
        {
            var filePath = Path.Combine(basePath, "prayers.json");
            var json = await File.ReadAllTextAsync(filePath);
            var prayers = JsonSerializer.Deserialize<List<Prayer>>(json, jsonOptions);

            if (prayers != null)
            {
                context.Prayers.AddRange(prayers);
                await context.SaveChangesAsync();
            }
        }
    }
}
