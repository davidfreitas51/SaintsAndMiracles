using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;

namespace API.Tests.Controllers;

public class ReligiousOrdersControllerTests : ControllerTestBase<ReligiousOrdersController>
{
    private Mock<IReligiousOrdersRepository> _ordersRepoMock = null!;
    private Mock<IReligiousOrdersService> _ordersServiceMock = null!;

    private void SetupController(bool authenticated = true)
    {
        _ordersRepoMock = CreateLooseMock<IReligiousOrdersRepository>();
        _ordersServiceMock = CreateLooseMock<IReligiousOrdersService>();

        if (authenticated)
        {
            SetupAuthenticatedController((userManager, signInManager) =>
                new ReligiousOrdersController(_ordersRepoMock.Object, _ordersServiceMock.Object, userManager.Object));
        }
        else
        {
            SetupUnauthenticatedController((userManager, signInManager) =>
                new ReligiousOrdersController(_ordersRepoMock.Object, _ordersServiceMock.Object, userManager.Object));
        }
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedOrders()
    {
        SetupController();

        var filters = new EntityFilters();

        var pagedResult = new PagedResult<ReligiousOrder>
        {
            Items =
            [
                new ReligiousOrder
                {
                    Id = 1,
                    Name = "Franciscan Order"
                }
            ],
            TotalCount = 1,
            PageSize = 10
        };

        _ordersRepoMock.Setup(r => r.GetAllAsync(filters))
            .ReturnsAsync(pagedResult);

        var result = await Controller.GetAll(filters);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedResult, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenExists()
    {
        SetupController();

        var order = new ReligiousOrder
        {
            Id = 1,
            Name = "Dominican Order"
        };

        _ordersRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var result = await Controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(order, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _ordersRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((ReligiousOrder?)null);

        var result = await Controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        var created = new ReligiousOrder
        {
            Id = 10,
            Name = dto.Name
        };

        _ordersServiceMock.Setup(s => s.CreateReligiousOrderAsync(dto, GetCurrentUserId()))
            .ReturnsAsync(created);

        var result = await Controller.Create(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ReligiousOrdersController.GetById), createdAt.ActionName);
        Assert.Same(created, createdAt.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenServiceFails()
    {
        SetupController();

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        _ordersServiceMock.Setup(s => s.CreateReligiousOrderAsync(dto, GetCurrentUserId()))
            .ReturnsAsync((ReligiousOrder?)null);

        var result = await Controller.Create(dto);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetupController(authenticated: false);

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        var result = await Controller.Create(dto);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        _ordersServiceMock.Setup(s =>
                s.UpdateReligiousOrderAsync(
                    1,
                    It.IsAny<NewReligiousOrderDto>(),
                    GetCurrentUserId()
                ))
            .ReturnsAsync(true);

        var dto = new NewReligiousOrderDto
        {
            Name = "Carmelite Order"
        };

        var result = await Controller.Update(1, dto);

        AssertNoContent(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _ordersServiceMock.Setup(s =>
                s.UpdateReligiousOrderAsync(
                    1,
                    It.IsAny<NewReligiousOrderDto>(),
                    GetCurrentUserId()
                ))
            .ReturnsAsync(false);

        var dto = new NewReligiousOrderDto
        {
            Name = "Carmelite Order"
        };

        var result = await Controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccessful()
    {
        SetupController();

        _ordersServiceMock.Setup(s => s.DeleteReligiousOrderAsync(1, GetCurrentUserId()))
            .ReturnsAsync(true);

        var result = await Controller.Delete(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _ordersServiceMock.Setup(s => s.DeleteReligiousOrderAsync(1, GetCurrentUserId()))
            .ReturnsAsync(false);

        var result = await Controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
