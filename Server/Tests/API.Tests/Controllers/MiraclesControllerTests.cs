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

public class MiraclesControllerTests : ControllerTestBase<MiraclesController>
{
    private Mock<IMiraclesRepository> _miracleRepoMock = null!;
    private Mock<IMiraclesService> _miracleServiceMock = null!;

    // =========================
    // Setup
    // =========================

    private void SetupController(bool authenticated = true)
    {
        _miracleRepoMock = CreateLooseMock<IMiraclesRepository>();
        _miracleServiceMock = CreateLooseMock<IMiraclesService>();

        if (authenticated)
        {
            SetupAuthenticatedController((userManager, signInManager) =>
                new MiraclesController(_miracleRepoMock.Object, _miracleServiceMock.Object, userManager.Object));
        }
        else
        {
            SetupUnauthenticatedController((userManager, signInManager) =>
                new MiraclesController(_miracleRepoMock.Object, _miracleServiceMock.Object, userManager.Object));
        }
    }

    // =========================
    // Factories
    // =========================

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

    // =========================
    // GET
    // =========================

    [Fact]
    public async Task GetAllMiracles_ShouldReturnOk()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<MiracleFilters>()))
            .ReturnsAsync(new PagedResult<Miracle>
            {
                Items = [],
                TotalCount = 0
            });

        var result = await Controller.GetAllMiracles(new MiracleFilters());

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Miracle?)null);

        var result = await Controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenFound()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateMiracle());

        var result = await Controller.GetById(1);

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetMiracleBySlug_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetBySlugAsync("slug"))
            .ReturnsAsync((Miracle?)null);

        var result = await Controller.GetMiracleBySlug("slug");

        Assert.IsType<NotFoundResult>(result);
    }

    // =========================
    // CREATE
    // =========================

    [Fact]
    public async Task CreateMiracle_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetupController(authenticated: false);

        var result = await Controller.CreateMiracle(NewMiracleDtoBuilder.Default().Build());

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task CreateMiracle_ShouldReturnConflict_WhenDuplicate()
    {
        SetupController();

        _miracleServiceMock.Setup(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync((int?)null);

        var result = await Controller.CreateMiracle(NewMiracleDtoBuilder.Default().Build());

        AssertConflict(result);
    }

    [Fact]
    public async Task CreateMiracle_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        _miracleServiceMock.Setup(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync(10);

        var result = await Controller.CreateMiracle(NewMiracleDtoBuilder.Default().Build());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(MiraclesController.GetById), created.ActionName);
    }

    [Fact]
    public async Task CreateMiracle_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewMiracleDtoBuilder.Default().Build();

        _miracleServiceMock.Setup(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync(10)
            .Verifiable();

        await Controller.CreateMiracle(dto);

        _miracleServiceMock.Verify(s => s.CreateMiracleAsync(It.IsAny<NewMiracleDto>(), GetCurrentUserId()), Times.Once);
    }

    // =========================
    // UPDATE
    // =========================

    [Fact]
    public async Task UpdateMiracle_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        SetupController(authenticated: false);

        var result = await Controller.UpdateMiracle(1, NewMiracleDtoBuilder.Default().Build());

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task UpdateMiracle_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _miracleServiceMock.Setup(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync(false);

        var result = await Controller.UpdateMiracle(1, NewMiracleDtoBuilder.Default().Build());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateMiracle_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        _miracleServiceMock.Setup(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync(true);

        var result = await Controller.UpdateMiracle(1, NewMiracleDtoBuilder.Default().Build());

        AssertNoContent(result);
    }

    [Fact]
    public async Task UpdateMiracle_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewMiracleDtoBuilder.Default().Build();

        _miracleServiceMock.Setup(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), GetCurrentUserId()))
            .ReturnsAsync(true)
            .Verifiable();

        await Controller.UpdateMiracle(1, dto);

        _miracleServiceMock.Verify(s => s.UpdateMiracleAsync(1, It.IsAny<NewMiracleDto>(), GetCurrentUserId()), Times.Once);
    }

    // =========================
    // DELETE
    // =========================

    [Fact]
    public async Task DeleteMiracle_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        SetupController(authenticated: false);

        var result = await Controller.DeleteMiracle(1);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task DeleteMiracle_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Miracle?)null);

        var result = await Controller.DeleteMiracle(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteMiracle_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        var miracle = CreateMiracle(slug: "miracle-slug");

        _miracleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(miracle);

        _miracleServiceMock.Setup(s => s.DeleteMiracleAsync(miracle.Slug, GetCurrentUserId()))
            .Returns(Task.CompletedTask);

        var result = await Controller.DeleteMiracle(1);

        AssertNoContent(result);
    }

    [Fact]
    public async Task DeleteMiracle_ShouldCallServiceWithCorrectUserId()
    {
        SetupController();

        var miracle = CreateMiracle(slug: "miracle-slug");

        _miracleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(miracle);

        _miracleServiceMock.Setup(s => s.DeleteMiracleAsync(miracle.Slug, GetCurrentUserId()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await Controller.DeleteMiracle(1);

        _miracleServiceMock.Verify(s => s.DeleteMiracleAsync(miracle.Slug, GetCurrentUserId()), Times.Once);
    }

    // =========================
    // COUNTRIES
    // =========================

    [Fact]
    public async Task GetMiracleCountries_ShouldReturnOk()
    {
        SetupController();

        _miracleRepoMock.Setup(r => r.GetCountriesAsync())
            .ReturnsAsync(["Italy", "France"]);

        var result = await Controller.GetMiracleCountries();

        AssertOkResult(result);
    }
}
