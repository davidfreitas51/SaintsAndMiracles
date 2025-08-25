using System.Text.Json;
using API.Helpers;
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
builder.Services.AddScoped<IAccountTokensService, AccountTokensService>();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
});

builder.Services.AddAuthorization();

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;

    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<AppUser>, EmailSender>();

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

app.Run();