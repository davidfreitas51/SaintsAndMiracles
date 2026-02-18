using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Common;

namespace API.Tests.Controllers;

public class DashboardControllerTests : UnitTestBase
{
    private Mock<ISaintsRepository> _saintsRepoMock = null!;
    private Mock<IMiraclesRepository> _miraclesRepoMock = null!;
    private Mock<IPrayersRepository> _prayersRepoMock = null!;
    private Mock<IRecentActivityRepository> _activityRepoMock = null!;
    private DashboardController _controller = null!;

    private void SetupController()
    {
        _saintsRepoMock = CreateLooseMock<ISaintsRepository>();
        _miraclesRepoMock = CreateLooseMock<IMiraclesRepository>();
        _prayersRepoMock = CreateLooseMock<IPrayersRepository>();
        _activityRepoMock = CreateLooseMock<IRecentActivityRepository>();

        // DashboardController needs real UserManager for Users.CountAsync()
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

        _controller = new DashboardController(
            _saintsRepoMock.Object,
            _miraclesRepoMock.Object,
            _prayersRepoMock.Object,
            _activityRepoMock.Object,
            userManager,
            NullLogger<DashboardController>.Instance);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnDashboardSummary()
    {
        SetupController();

        _saintsRepoMock.Setup(r => r.GetTotalSaintsAsync()).ReturnsAsync(10);
        _miraclesRepoMock.Setup(r => r.GetTotalMiraclesAsync()).ReturnsAsync(5);
        _prayersRepoMock.Setup(r => r.GetTotalPrayersAsync()).ReturnsAsync(20);

        var result = await _controller.GetSummary();

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<DashboardSummaryDto>(ok.Value);

        Assert.Equal(10, summary.TotalSaints);
        Assert.Equal(5, summary.TotalMiracles);
        Assert.Equal(20, summary.TotalPrayers);
        Assert.Equal(1, summary.TotalAccounts);
    }
}
