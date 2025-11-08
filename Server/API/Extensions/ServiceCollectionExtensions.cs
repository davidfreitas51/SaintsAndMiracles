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
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // Repositories
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

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
