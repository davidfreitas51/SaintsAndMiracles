using API.Helpers;
using Core.Interfaces;
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
        // Repositories
        services.AddScoped<ISaintsRepository, SaintsRepository>();
        services.AddScoped<IMiraclesRepository, MiraclesRepository>();
        services.AddScoped<IPrayersRepository, PrayersRepository>();
        services.AddScoped<IReligiousOrdersRepository, ReligiousOrdersRepository>();
        services.AddScoped<ITagsRepository, TagsRepository>();

        // Services
        services.AddScoped<ISaintsService, SaintsService>();
        services.AddScoped<IMiraclesService, MiraclesService>();
        services.AddScoped<IPrayersService, PrayersService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountTokensService, AccountTokensService>();
        services.AddScoped<IAccountManagementService, AccountManagementService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddSingleton<IEmailSender<AppUser>, EmailSender>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("defaultConnection"));
        });
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
