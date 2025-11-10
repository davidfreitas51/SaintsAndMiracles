using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


[assembly: InternalsVisibleTo("Infrastructure.Tests")]
public static class SeedData
{
    internal static string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/SeedData");
    internal static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
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

    internal static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "SuperAdmin" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                });
            }
        }
    }

    private static async Task SeedBootstrapToken(DataContext context, UserManager<AppUser> userManager, ITokenService tokenService)
    {
        if ((await userManager.GetUsersInRoleAsync("SuperAdmin")).Any()) return;

        var clearToken = tokenService.GenerateClearToken();
        var hash = tokenService.HashTokenBase64(clearToken);

        var bootstrapToken = new AccountToken
        {
            Hash = hash,
            Role = "SuperAdmin",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
            Purpose = "InitialAdminBootstrap",
            IssuedTo = "System"
        };

        context.AccountTokens.Add(bootstrapToken);
        await context.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=================================================");
        Console.WriteLine(" NO SUPER-ADMIN USERS FOUND ");
        Console.WriteLine(" Use this bootstrap token to create the first super admin account:");
        Console.WriteLine(clearToken);
        Console.WriteLine($" This token will expire at: {bootstrapToken.ExpiresAtUtc}");
        Console.WriteLine("=================================================");
        Console.ResetColor();
    }

    internal static async Task SeedTags(DataContext context)
    {
        var filePath = Path.Combine(basePath, "tags.json");
        var json = await File.ReadAllTextAsync(filePath);
        var tags = JsonSerializer.Deserialize<List<Tag>>(json, jsonOptions);
        if (tags == null || !tags.Any()) return;

        var existingTags = await context.Tags
            .Select(t => new { t.Name, t.TagType })
            .ToListAsync();

        var existingSet = new HashSet<string>(existingTags
            .Select(t => $"{t.Name.ToLowerInvariant()}|{(int)t.TagType}"));

        var tagsToAdd = tags
            .Where(tag => !existingSet.Contains($"{tag.Name.ToLowerInvariant()}|{(int)tag.TagType}"))
            .ToList();

        if (tagsToAdd.Any())
        {
            context.Tags.AddRange(tagsToAdd);
            await context.SaveChangesAsync();
        }
    }

    internal static async Task SeedSaints(DataContext context)
    {
        if (!context.Saints.Any())
        {
            var filePath = Path.Combine(basePath, "saints.json");
            var json = await File.ReadAllTextAsync(filePath);
            var saints = JsonSerializer.Deserialize<List<Saint>>(json, jsonOptions);
            if (saints == null || !saints.Any()) return;

            var allTags = await context.Tags.ToListAsync();

            foreach (var saint in saints)
            {
                if (saint.Tags == null) continue;

                var assignedTags = new List<Tag>();
                foreach (var tag in saint.Tags)
                {
                    var existingTag = allTags.FirstOrDefault(t =>
                        t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase) &&
                        t.TagType == tag.TagType);

                    if (existingTag != null)
                        assignedTags.Add(existingTag);
                }

                saint.Tags = assignedTags;
            }

            context.Saints.AddRange(saints);
            await context.SaveChangesAsync();
        }
    }

    internal static async Task SeedMiracles(DataContext context)
    {
        if (!context.Miracles.Any())
        {
            var filePath = Path.Combine(basePath, "miracles.json");
            var json = await File.ReadAllTextAsync(filePath);
            var miracles = JsonSerializer.Deserialize<List<Miracle>>(json, jsonOptions);
            if (miracles == null || !miracles.Any()) return;

            var allTags = await context.Tags.ToListAsync();

            foreach (var miracle in miracles)
            {
                if (miracle.Tags == null) continue;

                var assignedTags = new List<Tag>();
                foreach (var tag in miracle.Tags)
                {
                    var existingTag = allTags.FirstOrDefault(t =>
                        t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase) &&
                        t.TagType == tag.TagType);

                    if (existingTag != null)
                        assignedTags.Add(existingTag);
                }

                miracle.Tags = assignedTags;
            }

            context.Miracles.AddRange(miracles);
            await context.SaveChangesAsync();
        }
    }

    internal static async Task SeedPrayers(DataContext context)
    {
        if (!context.Prayers.Any())
        {
            var filePath = Path.Combine(basePath, "prayers.json");
            var json = await File.ReadAllTextAsync(filePath);
            var prayers = JsonSerializer.Deserialize<List<Prayer>>(json, jsonOptions);
            if (prayers == null || !prayers.Any()) return;

            var allTags = await context.Tags.ToListAsync();

            foreach (var prayer in prayers)
            {
                if (prayer.Tags == null) continue;

                var assignedTags = new List<Tag>();
                foreach (var tag in prayer.Tags)
                {
                    var existingTag = allTags.FirstOrDefault(t =>
                        t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase) &&
                        t.TagType == tag.TagType);

                    if (existingTag != null)
                        assignedTags.Add(existingTag);
                }

                prayer.Tags = assignedTags;
            }

            context.Prayers.AddRange(prayers);
            await context.SaveChangesAsync();
        }
    }
}
