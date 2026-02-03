using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers;

public class MiraclesControllerTests
{
    #region Setup

    private static MiraclesController CreateController(
        out Mock<IMiraclesRepository> repo,
        out Mock<IMiraclesService> service,
        out Mock<UserManager<AppUser>> userManagerMock,
        bool authenticated = true)
    {
        repo = new Mock<IMiraclesRepository>();
        service = new Mock<IMiraclesService>();

        var store = new Mock<IUserStore<AppUser>>();
        userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        var controller = new MiraclesController(
            repo.Object,
            service.Object,
            userManagerMock.Object
        );

        if (authenticated)
        {
            var user = new AppUser { Id = "user-1" };

            userManagerMock
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) },
                            "TestAuth"))
                }
            };
        }
        else
        {
            userManagerMock
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((AppUser?)null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        return controller;
    }

    #endregion

    #region Factories

    private static Miracle CreateMiracle(
        int id = 1,
        string? title = null,
        string? slug = null)
    {
        return new Miracle
        {
            Id = id,
            Title = title ?? "Sample Miracle",
            Slug = slug ?? "sample-miracle",
            Description = "Description",
            Country = "Italy",
            Century = 13,
            Image = "image.png",
            MarkdownPath = "miracle.md"
        };
    }

    private static NewMiracleDto CreateNewMiracleDto(
        string? title = null,
        string? description = null,
        string? markdownContent = null,
        string? country = null,
        string? image = null,
        int century = 0)
    {
        return new NewMiracleDto
        {
            Title = title ?? "New Miracle",
            Country = country ?? "Italy",
            Century = century,
            Image = image ?? "miracle.jpg",
            Description = description ?? "Miracle description",
            MarkdownContent = markdownContent ?? "miracle.md"
        };
    }


    #endregion

    #region GET

    [Fact]
    public async Task GetAllMiracles_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetAllAsync(It.IsAny<MiracleFilters>()))
            .ReturnsAsync(new PagedResult<Miracle>
            {
                Items = [],
                TotalCount = 0
            });

        var result = await controller.GetAllMiracles(new MiracleFilters());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Miracle?)null);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenFound()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateMiracle());

        var result = await controller.GetById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMiracleBySlug_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetBySlugAsync("slug"))
            .ReturnsAsync((Miracle?)null);

        var result = await controller.GetMiracleBySlug("slug");

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region CREATE

    [Fact]
    public async Task CreateMiracle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var controller = CreateController(out _, out _, out _, authenticated: false);

        var result = await controller.CreateMiracle(CreateNewMiracleDto());

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CreateMiracle_ShouldReturnConflict_WhenDuplicate()
    {
        var controller = CreateController(out _, out var service, out _);

        service.Setup(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), "user-1"))
            .ReturnsAsync((int?)null);

        var result = await controller.CreateMiracle(CreateNewMiracleDto());

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateMiracle_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service, out _);

        service.Setup(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), "user-1"))
            .ReturnsAsync(10);

        var result = await controller.CreateMiracle(CreateNewMiracleDto());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(MiraclesController.GetById), created.ActionName);
    }

    #endregion

    #region UPDATE

    [Fact]
    public async Task UpdateMiracle_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service, out _);

        service.Setup(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), "user-1"))
            .ReturnsAsync(false);

        var result = await controller.UpdateMiracle(1, CreateNewMiracleDto());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateMiracle_ShouldReturnNoContent_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service, out _);

        service.Setup(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), "user-1"))
            .ReturnsAsync(true);

        var result = await controller.UpdateMiracle(1, CreateNewMiracleDto());

        Assert.IsType<NoContentResult>(result);
    }

    #endregion

    #region DELETE

    [Fact]
    public async Task DeleteMiracle_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Miracle?)null);

        var result = await controller.DeleteMiracle(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteMiracle_ShouldReturnOk_WhenSuccessful()
    {
        var controller = CreateController(out var repo, out var service, out _);

        var miracle = CreateMiracle(slug: "miracle-slug");

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(miracle);

        service.Setup(s => s.DeleteMiracleAsync(miracle.Slug, "user-1"))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteMiracle(1);

        Assert.IsType<OkResult>(result);
    }

    #endregion

    #region COUNTRIES

    [Fact]
    public async Task GetMiracleCountries_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetCountriesAsync())
            .ReturnsAsync(["Italy", "France"]);

        var result = await controller.GetMiracleCountries();

        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}
