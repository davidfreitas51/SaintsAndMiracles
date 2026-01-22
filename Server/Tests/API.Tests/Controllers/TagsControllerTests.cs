using API.Controllers;
using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Infrastructure.Tests.Controllers;

public class TagsControllerTests
{
    private TagsController CreateController(
        out Mock<ITagsRepository> tagsRepo,
        out Mock<ITagsService> tagsService,
        bool authenticated = true
    )
    {
        tagsRepo = new Mock<ITagsRepository>();
        tagsService = new Mock<ITagsService>();

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

        var controller = new TagsController(
            tagsRepo.Object,
            tagsService.Object,
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
    public async Task GetAll_ShouldReturnPagedTags()
    {
        var controller = CreateController(out var repo, out _);

        var filters = new EntityFilters();

        var pagedResult = new PagedResult<Tag>
        {
            Items =
            [
                new Tag
                {
                    Id = 1,
                    Name = "Miracle",
                    TagType = default! // ✅ satisfy required member
                }
            ],
            TotalCount = 1
        };

        repo.Setup(r => r.GetAllAsync(filters))
            .ReturnsAsync(pagedResult);

        var result = await controller.GetAll(filters);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedResult, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnTag_WhenExists()
    {
        var controller = CreateController(out var repo, out _);

        var tag = new Tag
        {
            Id = 1,
            Name = "Saint",
            TagType = default!
        };

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(tag);

        var result = await controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(tag, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out var repo, out _);

        repo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Tag?)null);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        var dto = new NewTagDto
        {
            TagType = default!
        };

        var created = new Tag
        {
            Id = 10,
            Name = "Prayer",
            TagType = default!
        };

        service.Setup(s => s.CreateTagAsync(dto, "user-1"))
            .ReturnsAsync(created);

        var result = await controller.Create(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(TagsController.GetById), createdAt.ActionName);
        Assert.Same(created, createdAt.Value);
    }


    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenServiceFails()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s => s.CreateTagAsync(It.IsAny<NewTagDto>(), "user-1"))
            .ReturnsAsync((Tag?)null);

        var result = await controller.Create(new NewTagDto
        {
            TagType = TagType.Saint
        });

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var controller = CreateController(out _, out _, authenticated: false);

        var result = await controller.Create(new NewTagDto
        {
            TagType = TagType.Saint
        });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s =>
                s.UpdateTagAsync(
                    1,
                    It.IsAny<NewTagDto>(),
                    "user-1"
                ))
            .ReturnsAsync(true);

        var result = await controller.Update(1, new NewTagDto
        {
            TagType = TagType.Saint
        });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s =>
                s.UpdateTagAsync(
                    1,
                    It.IsAny<NewTagDto>(),
                    "user-1"
                ))
            .ReturnsAsync(false);

        var result = await controller.Update(1, new NewTagDto
        {
            TagType = TagType.Saint
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccessful()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s => s.DeleteTagAsync(1, "user-1"))
            .ReturnsAsync(true);

        var result = await controller.Delete(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var controller = CreateController(out _, out var service);

        service.Setup(s => s.DeleteTagAsync(1, "user-1"))
            .ReturnsAsync(false);

        var result = await controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
