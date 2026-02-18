using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;
using Tests.Common.Builders;

namespace API.Tests.Controllers;

public class PrayersControllerTests : ControllerTestBase<PrayersController>
{
    private Mock<IPrayersRepository> _prayerRepoMock = null!;
    private Mock<IPrayersService> _prayerServiceMock = null!;

    // =========================
    // Setup
    // =========================

    private void SetupController(bool authenticated = true)
    {
        _prayerRepoMock = CreateLooseMock<IPrayersRepository>();
        _prayerServiceMock = CreateLooseMock<IPrayersService>();

        if (authenticated)
        {
            SetupAuthenticatedController((userManager, signInManager) =>
                new PrayersController(_prayerRepoMock.Object, _prayerServiceMock.Object, userManager.Object));
        }
        else
        {
            SetupUnauthenticatedController((userManager, signInManager) =>
                new PrayersController(_prayerRepoMock.Object, _prayerServiceMock.Object, userManager.Object));
        }
    }

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

    // =========================
    // GET
    // =========================

    [Fact]
    public async Task GetAllPrayers_ShouldReturnOk()
    {
        SetupController();

        _prayerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<PrayerFilters>()))
            .ReturnsAsync(new PagedResult<Prayer>
            {
                Items = [],
                TotalCount = 0
            });

        var result = await Controller.GetAllPrayers(new PrayerFilters());

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenPrayerExists()
    {
        SetupController();
        var prayer = CreatePrayer();

        _prayerRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(prayer);

        var result = await Controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(prayer, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        SetupController();

        _prayerRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Prayer?)null);

        var result = await Controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPrayerBySlug_ShouldReturnOk_WhenExists()
    {
        SetupController();
        var prayer = CreatePrayer();

        _prayerRepoMock.Setup(r => r.GetBySlugAsync("slug"))
            .ReturnsAsync(prayer);

        var result = await Controller.GetPrayerBySlug("slug");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(prayer, ok.Value);
    }

    // =========================
    // POST
    // =========================

    [Fact]
    public async Task CreatePrayer_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.CreatePrayerAsync(dto, GetCurrentUserId()))
            .ReturnsAsync(10);

        var result = await Controller.CreatePrayer(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PrayersController.GetById), created.ActionName);
    }

    [Fact]
    public async Task CreatePrayer_ShouldReturnConflict_WhenDuplicate()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.CreatePrayerAsync(dto, GetCurrentUserId()))
            .ReturnsAsync((int?)null);

        var result = await Controller.CreatePrayer(dto);

        AssertConflict(result);
    }

    [Fact]
    public async Task CreatePrayer_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.CreatePrayerAsync(It.IsAny<NewPrayerDto>(), GetCurrentUserId()))
            .ReturnsAsync(10)
            .Verifiable();

        await Controller.CreatePrayer(dto);

        _prayerServiceMock.Verify(s => s.CreatePrayerAsync(It.IsAny<NewPrayerDto>(), GetCurrentUserId()), Times.Once);
    }

    // =========================
    // PUT
    // =========================

    [Fact]
    public async Task UpdatePrayer_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.UpdatePrayerAsync(1, dto, GetCurrentUserId()))
            .ReturnsAsync(true);

        var result = await Controller.UpdatePrayer(1, dto);

        AssertNoContent(result);
    }

    [Fact]
    public async Task UpdatePrayer_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.UpdatePrayerAsync(1, dto, GetCurrentUserId()))
            .ReturnsAsync(false);

        var result = await Controller.UpdatePrayer(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdatePrayer_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewPrayerDtoBuilder.Default().Build();

        _prayerServiceMock.Setup(s => s.UpdatePrayerAsync(1, It.IsAny<NewPrayerDto>(), GetCurrentUserId()))
            .ReturnsAsync(true)
            .Verifiable();

        await Controller.UpdatePrayer(1, dto);

        _prayerServiceMock.Verify(s => s.UpdatePrayerAsync(1, It.IsAny<NewPrayerDto>(), GetCurrentUserId()), Times.Once);
    }

    // =========================
    // DELETE
    // =========================

    [Fact]
    public async Task DeletePrayer_ShouldReturnOk_WhenExists()
    {
        SetupController();
        var prayer = CreatePrayer();

        _prayerRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(prayer);

        var result = await Controller.DeletePrayer(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeletePrayer_ShouldReturnNotFound_WhenPrayerDoesNotExist()
    {
        SetupController();

        _prayerRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Prayer?)null);

        var result = await Controller.DeletePrayer(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePrayer_ShouldCallServiceWithCorrectUserId()
    {
        SetupController();
        var prayer = CreatePrayer(slug: "prayer-slug");

        _prayerRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(prayer);

        _prayerServiceMock.Setup(s => s.DeletePrayerAsync(prayer.Slug, GetCurrentUserId()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await Controller.DeletePrayer(1);

        _prayerServiceMock.Verify(s => s.DeletePrayerAsync(prayer.Slug, GetCurrentUserId()), Times.Once);
    }

    // =========================
    // TAGS
    // =========================

    [Fact]
    public async Task GetPrayerTags_ShouldReturnOk()
    {
        SetupController();

        _prayerRepoMock.Setup(r => r.GetTagsAsync())
            .ReturnsAsync([]);

        var result = await Controller.GetPrayerTags();

        AssertOkResult(result);
    }
}
