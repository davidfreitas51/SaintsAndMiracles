using API.Controllers;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers;

public class PrayersControllerTests
{
    // =========================
    // Factories
    // =========================

    private static Prayer CreatePrayer(
        int id = 1,
        string? title = null,
        string? slug = null,
        string? description = null,
        string? image = null,
        string? markdownPath = null)
    {
        return new Prayer
        {
            Id = id,
            Title = title ?? "Sample Prayer",
            Slug = slug ?? "sample-prayer",
            Description = description ?? "Prayer description",
            Image = image ?? "image.png",
            MarkdownPath = markdownPath ?? "prayer.md"
        };
    }

    private static NewPrayerDto CreateNewPrayerDto(
    string? title = null,
    string? description = null,
    string? markdownContent = null)
    {
        return new NewPrayerDto
        {
            Title = title ?? "Prayer title",
            Description = description ?? "Prayer description",
            MarkdownContent = markdownContent ?? "prayer.md",
            Image = "image.webp"
        };
    }


    private static PrayersController CreateController(
        out Mock<IPrayersRepository> repo,
        out Mock<IPrayersService> service,
        out Mock<UserManager<AppUser>> userManager)
    {
        repo = new Mock<IPrayersRepository>();
        service = new Mock<IPrayersService>();

        var store = new Mock<IUserStore<AppUser>>();
        userManager = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );

        var controller = new PrayersController(
            repo.Object,
            service.Object,
            userManager.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "user-1")],
                        "TestAuth"
                    )
                )
            }
        };

        return controller;
    }

    // =========================
    // GET
    // =========================

    [Fact]
    public async Task GetAllPrayers_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetAllAsync(It.IsAny<PrayerFilters>()))
            .ReturnsAsync(new PagedResult<Prayer>
            {
                Items = [],
                TotalCount = 0
            });

        var result = await controller.GetAllPrayers(new PrayerFilters());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenPrayerExists()
    {
        var controller = CreateController(out var repo, out _, out _);
        var prayer = CreatePrayer();

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(prayer);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(prayer, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Prayer?)null);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPrayerBySlug_ShouldReturnOk_WhenExists()
    {
        var controller = CreateController(out var repo, out _, out _);
        var prayer = CreatePrayer();

        repo.Setup(r => r.GetBySlugAsync("slug"))
            .ReturnsAsync(prayer);

        var result = await controller.GetPrayerBySlug("slug");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(prayer, ok.Value);
    }

    // =========================
    // POST
    // =========================

    [Fact]
    public async Task CreatePrayer_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service, out var userManager);

        var dto = CreateNewPrayerDto();

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        service.Setup(s => s.CreatePrayerAsync(dto, "user-1"))
            .ReturnsAsync(10);

        var result = await controller.CreatePrayer(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PrayersController.GetById), created.ActionName);
    }

    [Fact]
    public async Task CreatePrayer_ShouldReturnConflict_WhenDuplicate()
    {
        var controller = CreateController(out _, out var service, out var userManager);

        var dto = CreateNewPrayerDto();

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        service.Setup(s => s.CreatePrayerAsync(dto, "user-1"))
            .ReturnsAsync((int?)null);

        var result = await controller.CreatePrayer(dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // =========================
    // PUT
    // =========================

    [Fact]
    public async Task UpdatePrayer_ShouldReturnNoContent_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service, out var userManager);

        var dto = CreateNewPrayerDto();

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        service.Setup(s => s.UpdatePrayerAsync(1, dto, "user-1"))
            .ReturnsAsync(true);

        var result = await controller.UpdatePrayer(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdatePrayer_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        var controller = CreateController(out _, out var service, out var userManager);

        var dto = CreateNewPrayerDto();

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        service.Setup(s => s.UpdatePrayerAsync(1, dto, "user-1"))
            .ReturnsAsync(false);

        var result = await controller.UpdatePrayer(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    // =========================
    // DELETE
    // =========================

    [Fact]
    public async Task DeletePrayer_ShouldReturnOk_WhenExists()
    {
        var controller = CreateController(out var repo, out var service, out var userManager);
        var prayer = CreatePrayer();

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(prayer);

        var result = await controller.DeletePrayer(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeletePrayer_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        var controller = CreateController(out var repo, out _, out var userManager);

        userManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("user-1");

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Prayer?)null);

        var result = await controller.DeletePrayer(1);

        Assert.IsType<NotFoundResult>(result);
    }

    // =========================
    // TAGS
    // =========================

    [Fact]
    public async Task GetPrayerTags_ShouldReturnOk()
    {
        var controller = CreateController(out var repo, out _, out _);

        repo.Setup(r => r.GetTagsAsync())
            .ReturnsAsync([]);

        var result = await controller.GetPrayerTags();

        Assert.IsType<OkObjectResult>(result);
    }
}
