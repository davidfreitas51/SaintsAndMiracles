using System.Security.Claims;
using API.Controllers;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Controllers;

public class ReligiousOrdersControllerTests
{
    private ReligiousOrdersController CreateController(
        out Mock<IReligiousOrdersRepository> ordersRepo,
        out Mock<IReligiousOrdersService> ordersService,
        bool authenticated = true
    )
    {
        ordersRepo = new Mock<IReligiousOrdersRepository>();
        ordersService = new Mock<IReligiousOrdersService>();

        var options = new DbContextOptionsBuilder<IdentityDbContext<AppUser>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IdentityDbContext<AppUser>(options);

        var user = new AppUser
        {
            Id = "user-1",
            Email = "user@test.com",
            UserName = "user@test.com",
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
            [],
            [],
            null!,
            null!,
            null!,
            null!
        );

        var controller = new ReligiousOrdersController(
            ordersRepo.Object,
            ordersService.Object,
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
                            "TestAuth"
                        )
                    )
                    : new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedOrders()
    {
        var controller = CreateController(out var repo, out _);

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

        repo.Setup(r => r.GetAllAsync(filters))
            .ReturnsAsync(pagedResult);

        var result = await controller.GetAll(filters);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedResult, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenExists()
    {
        var controller = CreateController(out var repo, out _);

        var order = new ReligiousOrder
        {
            Id = 1,
            Name = "Dominican Order"
        };

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(order, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((ReligiousOrder?)null);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        var created = new ReligiousOrder
        {
            Id = 10,
            Name = dto.Name
        };

        service.Setup(s => s.CreateReligiousOrderAsync(dto, "user-1"))
            .ReturnsAsync(created);

        var result = await controller.Create(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ReligiousOrdersController.GetById), createdAt.ActionName);
        Assert.Same(created, createdAt.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenServiceFails()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        service.Setup(s => s.CreateReligiousOrderAsync(dto, "user-1"))
            .ReturnsAsync((ReligiousOrder?)null);

        var result = await controller.Create(dto);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var controller = CreateController(out _, out _, authenticated: false);

        var dto = new NewReligiousOrderDto
        {
            Name = "Benedictine Order"
        };

        var result = await controller.Create(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s =>
                s.UpdateReligiousOrderAsync(
                    1,
                    It.IsAny<NewReligiousOrderDto>(),
                    "user-1"
                ))
            .ReturnsAsync(true);

        var dto = new NewReligiousOrderDto
        {
            Name = "Carmelite Order"
        };

        var result = await controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s =>
                s.UpdateReligiousOrderAsync(
                    1,
                    It.IsAny<NewReligiousOrderDto>(),
                    "user-1"
                ))
            .ReturnsAsync(false);

        var dto = new NewReligiousOrderDto
        {
            Name = "Carmelite Order"
        };

        var result = await controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s => s.DeleteReligiousOrderAsync(1, "user-1"))
            .ReturnsAsync(true);

        var result = await controller.Delete(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s => s.DeleteReligiousOrderAsync(1, "user-1"))
            .ReturnsAsync(false);

        var result = await controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
