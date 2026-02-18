using API.Controllers;
using Core.Interfaces;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;
using Tests.Common.Builders;

namespace API.Tests.Controllers;

/// <summary>
/// Test suite for SaintsController.
/// Uses ControllerTestBase for reduced boilerplate and consistent test structure.
/// </summary>
public class SaintsControllerTests : ControllerTestBase<SaintsController>
{
    private Mock<ISaintsRepository> _saintRepoMock = null!;
    private Mock<ISaintsService> _saintServiceMock = null!;

    /// <summary>
    /// Helper to setup controller with mocked dependencies.
    /// </summary>
    private void SetupController(bool authenticated = true)
    {
        _saintRepoMock = CreateLooseMock<ISaintsRepository>();
        _saintServiceMock = CreateLooseMock<ISaintsService>();

        if (authenticated)
        {
            SetupAuthenticatedController(
                (userManager, signInManager) =>
                {
                    // Configure UserManager to return authenticated user
                    userManager
                        .Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                        .ReturnsAsync(AuthenticatedUser);

                    return new SaintsController(
                        _saintRepoMock.Object,
                        _saintServiceMock.Object,
                        userManager.Object
                    );
                }
            );
        }
        else
        {
            SetupUnauthenticatedController(
                (userManager, signInManager) => new SaintsController(
                    _saintRepoMock.Object,
                    _saintServiceMock.Object,
                    userManager.Object
                )
            );
        }
    }

    // -------------------- GET -------------------

    [Fact]
    public async Task GetAllSaints_ShouldReturnOk()
    {
        SetupController();
        _saintRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<SaintFilters>()))
            .ReturnsAsync(EmptyPagedResult<Saint>());

        var result = await Controller.GetAllSaints(new SaintFilters());

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetAllSaints_ShouldReturnWithData_WhenSaintsExist()
    {
        SetupController();
        var saints = new List<Saint>
        {
            CreateSaint(1, "Francis"),
            CreateSaint(2, "Teresa")
        };
        var pagedResult = new PagedResult<Saint>
        {
            Items = saints,
            TotalCount = 2
        };

        _saintRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<SaintFilters>()))
            .ReturnsAsync(pagedResult);

        var result = await Controller.GetAllSaints(new SaintFilters());

        AssertOkResult(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenSaintExists()
    {
        SetupController();
        var saint = CreateSaint();

        _saintRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(saint);

        var result = await Controller.GetById(1);

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Saint?)null);

        var result = await Controller.GetById(1);

        AssertNotFound(result);
    }

    [Fact]
    public async Task GetSaintBySlug_ShouldReturnOk_WhenFound()
    {
        SetupController();
        var saint = CreateSaint();

        _saintRepoMock
            .Setup(r => r.GetBySlugAsync("francis"))
            .ReturnsAsync(saint);

        var result = await Controller.GetSaintBySlug("francis");

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetSaintBySlug_ShouldReturnNotFound_WhenNotFound()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetBySlugAsync("nonexistent"))
            .ReturnsAsync((Saint?)null);

        var result = await Controller.GetSaintBySlug("nonexistent");

        AssertNotFound(result);
    }

    // -------------------- CREATE --------------------

    [Fact]
    public async Task CreateSaint_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        SetupController(authenticated: false);

        var dto = NewSaintDtoBuilder.Default().Build();

        var result = await Controller.CreateSaint(dto);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task CreateSaint_ShouldReturnConflict_WhenDuplicate()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();

        _saintServiceMock
            .Setup(s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), It.IsAny<string>()))
            .ReturnsAsync((int?)null);

        var result = await Controller.CreateSaint(dto);

        AssertConflict(result);
    }

    [Fact]
    public async Task CreateSaint_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();

        _saintServiceMock
            .Setup(s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), It.IsAny<string>()))
            .ReturnsAsync(10);

        var result = await Controller.CreateSaint(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(SaintsController.GetById), createdResult.ActionName);
        Assert.Equal(10, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task CreateSaint_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();
        var userId = GetCurrentUserId();

        _saintServiceMock
            .Setup(s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), userId))
            .ReturnsAsync(1);

        await Controller.CreateSaint(dto);

        _saintServiceMock.Verify(
            s => s.CreateSaintAsync(It.IsAny<NewSaintDto>(), userId),
            Times.Once
        );
    }

    // -------------------- UPDATE --------------------

    [Fact]
    public async Task UpdateSaint_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        SetupController(authenticated: false);

        var dto = NewSaintDtoBuilder.Default().Build();

        var result = await Controller.UpdateSaint(1, dto);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task UpdateSaint_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();

        _saintServiceMock
            .Setup(s => s.UpdateSaintAsync(1, dto, It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await Controller.UpdateSaint(1, dto);

        AssertNotFound(result);
    }

    [Fact]
    public async Task UpdateSaint_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();

        _saintServiceMock
            .Setup(s => s.UpdateSaintAsync(1, dto, It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await Controller.UpdateSaint(1, dto);

        AssertNoContent(result);
    }

    [Fact]
    public async Task UpdateSaint_ShouldPassCorrectUserIdToService()
    {
        SetupController();

        var dto = NewSaintDtoBuilder.Default().Build();
        var userId = GetCurrentUserId();

        _saintServiceMock
            .Setup(s => s.UpdateSaintAsync(1, dto, userId))
            .ReturnsAsync(true);

        await Controller.UpdateSaint(1, dto);

        _saintServiceMock.Verify(
            s => s.UpdateSaintAsync(1, dto, userId),
            Times.Once
        );
    }

    // -------------------- DELETE --------------------

    [Fact]
    public async Task DeleteSaint_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        SetupController(authenticated: false);

        var result = await Controller.DeleteSaint(1);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task DeleteSaint_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Saint?)null);

        var result = await Controller.DeleteSaint(1);

        AssertNotFound(result);
    }

    [Fact]
    public async Task DeleteSaint_ShouldReturnOk_WhenSuccessful()
    {
        SetupController();

        var saint = CreateSaint();

        _saintRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(saint);

        var result = await Controller.DeleteSaint(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteSaint_ShouldCallServiceWithCorrectUserId()
    {
        SetupController();

        var saint = CreateSaint();
        var userId = GetCurrentUserId();

        _saintRepoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(saint);

        await Controller.DeleteSaint(1);

        _saintServiceMock.Verify(
            s => s.DeleteSaintAsync(1, userId),
            Times.Once
        );
    }

    // -------------------- EXTRA --------------------

    [Fact]
    public async Task GetSaintCountries_ShouldReturnOk()
    {
        SetupController();

        var countries = new List<string> { "Italy", "France", "Spain" };

        _saintRepoMock
            .Setup(r => r.GetCountriesAsync())
            .ReturnsAsync(countries);

        var result = await Controller.GetSaintCountries();

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetSaintsOfTheDay_ShouldReturnNoContent_WhenEmpty()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetSaintsOfTheDayAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<Saint>());

        var result = await Controller.GetSaintsOfTheDay();

        AssertNoContent(result);
    }

    [Fact]
    public async Task GetSaintsOfTheDay_ShouldReturnOk_WhenSaintsExist()
    {
        SetupController();

        var saints = new List<Saint> { CreateSaint() };

        _saintRepoMock
            .Setup(r => r.GetSaintsOfTheDayAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(saints);

        var result = await Controller.GetSaintsOfTheDay();

        AssertOkResult(result);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldReturnNoContent_WhenEmpty()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Saint>());

        var result = await Controller.GetUpcomingFeasts();

        AssertNoContent(result);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldReturnOk_WhenSaintsExist()
    {
        SetupController();

        var saints = new List<Saint>
        {
            CreateSaint(1, "Joan of Arc"),
            CreateSaint(2, "Francis of Assisi")
        };

        _saintRepoMock
            .Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(saints);

        var result = await Controller.GetUpcomingFeasts();

        AssertOkResult(result);
        var okResult = result as OkObjectResult;
        var value = Assert.IsAssignableFrom<List<Saint>>(okResult?.Value);
        Assert.Equal(2, value.Count);
    }

    [Fact]
    public async Task GetUpcomingFeasts_ShouldCallRepository()
    {
        SetupController();

        _saintRepoMock
            .Setup(r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Saint>());

        await Controller.GetUpcomingFeasts();

        _saintRepoMock.Verify(
            r => r.GetUpcomingFeasts(It.IsAny<DateOnly>(), It.IsAny<int>()),
            Times.Once
        );
    }

    // -------------------- HELPERS --------------------

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
