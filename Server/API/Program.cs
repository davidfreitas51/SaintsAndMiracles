using System.Text.Json;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

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

builder.Services.AddScoped<ISaintsRepository, SaintsRepository>();
builder.Services.AddScoped<ISaintsService, SaintsService>();
builder.Services.AddScoped<IMiraclesRepository, MiraclesRepository>();
builder.Services.AddScoped<IMiraclesService, MiraclesService>();
builder.Services.AddScoped<IPrayersRepository, PrayersRepository>();
builder.Services.AddScoped<IPrayersService, PrayersService>();
builder.Services.AddScoped<IReligiousOrdersRepository, ReligiousOrdersRepository>();
builder.Services.AddScoped<ITagsRepository, TagsRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IInviteService, InviteService>();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
});

builder.Services.AddAuthorization();

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<AppUser>, DummyEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var tokenService = services.GetRequiredService<ITokenService>();

    await SeedData.SeedAsync(context, roleManager, userManager, tokenService);
}

app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();

app.Run();

public class DummyEmailSender : IEmailSender<AppUser>
{
    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendEmailAsync(AppUser user, string subject, string htmlMessage)
    {
        // Do nothing
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }
}
