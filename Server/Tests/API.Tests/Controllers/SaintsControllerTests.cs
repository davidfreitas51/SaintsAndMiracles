using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers;

public class SaintsControllerTests
{
    private SaintsController CreateController(
        out Mock<ISaintsRepository> repo,
        out Mock<ISaintsService> service,
        bool authenticated = true)
    {
        repo = new Mock<ISaintsRepository>();
        service = new Mock<ISaintsService>();

        var options = new DbContextOptionsBuilder<IdentityDbContext<AppUser>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IdentityDbContext<AppUser>(options);

        var user = new AppUser
        {
            Id = "user-1",
            UserName = "test@test.com",
            Email = "test@test.com",
            EmailConfirmed = true
        };

        if (authenticated)
        {
            context.Users.Add(user);
            context.SaveChanges();
        }

        var store = new UserStore<AppUser>(context);
        var userManager = new UserManager<AppUser>(
            store,
            null!,
            new PasswordHasher<AppUser>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            null!,
            null!,
            null!,
            null!
        );

        var controller = new SaintsController(
            repo.Object,
            service.Object,
            userManager
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = authenticated
                    ? new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) },
                            "TestAuth"))
                    : new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        return controller;
    }

    // -------------------- GET -------------------

    [Fact]
    public async Task GetAllSaints_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetAllAsync(It.IsAny<SaintFilters>()))
            .ReturnsAsync(EmptyPagedResult<Saint>());

        var result = await controller.GetAllSaints(new SaintFilters());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Saint?)null);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetSaintBySlug_ShouldReturnOk_WhenFound()
    {
        var controller = CreateController(out var repo, out _);

        var saint = CreateSaint();

        repo.Setup(r => r.GetBySlugAsync("francis"))
            .ReturnsAsync(saint);

        var result = await controller.GetSaintBySlug("francis");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(saint, ok.Value);
    }

    // -------------------- CREATE --------------------

    [Fact]
    public async Task CreateSaint_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var controller = CreateController(out _, out _, authenticated: false);

        var dto = new NewSaintDto
        {
            Name = "Francis",
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownContent = "saints/francis.md"
        };

        var result = await controller.CreateSaint(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateSaint_ShouldReturnConflict_WhenDuplicate()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewSaintDto
        {
            Name = "Francis",
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownContent = "saints/francis.md"
        };

        service.Setup(s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), "user-1"))
            .ReturnsAsync((int?)null);

        var result = await controller.CreateSaint(dto);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("A saint with the same name already exists.", conflict.Value);
    }

    [Fact]
    public async Task CreateSaint_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewSaintDto
        {
            Name = "Francis",
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownContent = "saints/francis.md"
        };

        service.Setup(s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), "user-1"))
            .ReturnsAsync(10);

        var result = await controller.CreateSaint(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(SaintsController.GetById), created.ActionName);
        Assert.Equal(10, created.RouteValues!["id"]);
    }

    // -------------------- UPDATE --------------------

    [Fact]
    public async Task UpdateSaint_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewSaintDto
        {
            Name = "Francis Updated",
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownContent = "saints/francis.md"
        };

        service.Setup(s => s.UpdateSaintAsync(1, dto, "user-1"))
            .ReturnsAsync(false);

        var result = await controller.UpdateSaint(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateSaint_ShouldReturnNoContent_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewSaintDto
        {
            Name = "Francis Updated",
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownContent = "saints/francis.md"
        };

        service.Setup(s => s.UpdateSaintAsync(1, dto, "user-1"))
            .ReturnsAsync(true);

        var result = await controller.UpdateSaint(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    // -------------------- DELETE --------------------

    [Fact]
    public async Task DeleteSaint_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Saint?)null);

        var result = await controller.DeleteSaint(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSaint_ShouldReturnOk_WhenSuccessful()
    {
        var controller = CreateController(out var repo, out var service);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateSaint());

        var result = await controller.DeleteSaint(1);

        Assert.IsType<OkResult>(result);
        service.Verify(s => s.DeleteSaintAsync(1, "user-1"), Times.Once);
    }

    // -------------------- EXTRA --------------------

    [Fact]
    public async Task GetSaintCountries_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetCountriesAsync())
            .ReturnsAsync(new List<string> { "Italy", "France" });

        var result = await controller.GetSaintCountries();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetSaintsOfTheDay_ShouldReturnNoContent_WhenEmpty()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetSaintsOfTheDayAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<Saint>());

        var result = await controller.GetSaintsOfTheDay();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldReturnNoContent_WhenEmpty()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Saint>());

        var result = await controller.GetUpcomingFeasts();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldReturnOk_WhenSaintsExist()
    {
        var controller = CreateController(out var repo, out _);

        var saints = new List<Saint>
        {
            CreateSaint(1, "Joan of Arc", "joan-of-arc"),
            CreateSaint(2, "Francis of Assisi", "francis-of-assisi")
        };

        repo.Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(saints);

        var result = await controller.GetUpcomingFeasts();

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<List<Saint>>(ok.Value);

        Assert.Equal(2, value.Count);
        Assert.Equal("Joan of Arc", value[0].Name);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldCallRepository()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Saint> { CreateSaint() });

        await controller.GetUpcomingFeasts();

        repo.Verify(
            r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()),
            Times.Once
        );
    }

    private static Saint CreateSaint(
        int id = 1,
        string name = "Francis",
        string slug = "francis")
    {
        return new Saint
        {
            Id = id,
            Name = name,
            Slug = slug,
            Country = "Italy",
            Century = 13,
            Image = "image.jpg",
            Description = "Saint description",
            MarkdownPath = "saints/francis.md"
        };
    }

    private static PagedResult<T> EmptyPagedResult<T>() =>
        new()
        {
            Items = new List<T>(),
            TotalCount = 0
        };
}
