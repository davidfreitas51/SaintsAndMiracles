using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Controllers;

public class DashboardControllerTests
{
    private DashboardController CreateController(
        out Mock<ISaintsRepository> saintsRepo,
        out Mock<IMiraclesRepository> miraclesRepo,
        out Mock<IPrayersRepository> prayersRepo,
        out Mock<IRecentActivityRepository> activityRepo)
    {
        saintsRepo = new Mock<ISaintsRepository>();
        miraclesRepo = new Mock<IMiraclesRepository>();
        prayersRepo = new Mock<IPrayersRepository>();
        activityRepo = new Mock<IRecentActivityRepository>();

        // 🔹 EF Core InMemory for UserManager
        var options = new DbContextOptionsBuilder<IdentityDbContext<AppUser>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IdentityDbContext<AppUser>(options);
        context.Users.Add(new AppUser
        {
            Id = "1",
            Email = "user@test.com",
            EmailConfirmed = true
        });
        context.SaveChanges();

        var store = new UserStore<AppUser>(context);
        var userManager = new UserManager<AppUser>(
            store,
            null!,
            new PasswordHasher<AppUser>(),
            [],
            [],
            null!,
            null!,
            null!,
            null!
        );

        return new DashboardController(
            saintsRepo.Object,
            miraclesRepo.Object,
            prayersRepo.Object,
            activityRepo.Object,
            userManager
        );
    }

    [Fact]
    public async Task GetSummary_ShouldReturnDashboardSummary()
    {
        var controller = CreateController(
            out var saintsRepo,
            out var miraclesRepo,
            out var prayersRepo,
            out _);

        saintsRepo.Setup(r => r.GetTotalSaintsAsync()).ReturnsAsync(10);
        miraclesRepo.Setup(r => r.GetTotalMiraclesAsync()).ReturnsAsync(5);
        prayersRepo.Setup(r => r.GetTotalPrayersAsync()).ReturnsAsync(20);

        var result = await controller.GetSummary();

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<DashboardSummaryDto>(ok.Value);

        Assert.Equal(10, summary.TotalSaints);
        Assert.Equal(5, summary.TotalMiracles);
        Assert.Equal(20, summary.TotalPrayers);
        Assert.Equal(1, summary.TotalAccounts);
    }
}
