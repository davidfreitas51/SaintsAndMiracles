using API.Controllers;
using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;

namespace API.Tests.Controllers;

public class TagsControllerTests : ControllerTestBase<TagsController>
{
    private Mock<ITagsRepository> _tagsRepoMock = null!;
    private Mock<ITagsService> _tagsServiceMock = null!;

    private void SetupController(bool authenticated = true)
    {
        _tagsRepoMock = CreateLooseMock<ITagsRepository>();
        _tagsServiceMock = CreateLooseMock<ITagsService>();

        if (authenticated)
        {
            SetupAuthenticatedController((userManager, signInManager) =>
                new TagsController(_tagsRepoMock.Object, _tagsServiceMock.Object, userManager.Object));
        }
        else
        {
            SetupUnauthenticatedController((userManager, signInManager) =>
                new TagsController(_tagsRepoMock.Object, _tagsServiceMock.Object, userManager.Object));
        }
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedTags()
    {
        SetupController();

        var filters = new EntityFilters();

        var pagedResult = new PagedResult<Tag>
        {
            Items =
            [
                new Tag
                {
                    Id = 1,
                    Name = "Miracle",
                    TagType = TagType.Miracle
                }
            ],
            TotalCount = 1
        };

        _tagsRepoMock.Setup(r => r.GetAllAsync(filters))
            .ReturnsAsync(pagedResult);

        var result = await Controller.GetAll(filters);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(pagedResult, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnTag_WhenExists()
    {
        SetupController();

        var tag = new Tag
        {
            Id = 1,
            Name = "Saint",
            TagType = TagType.Saint
        };

        _tagsRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(tag);

        var result = await Controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(tag, ok.Value);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _tagsRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Tag?)null);

        var result = await Controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        var dto = new NewTagDto
        {
            Name = "Prayer",
            TagType = TagType.Miracle
        };

        var created = new Tag
        {
            Id = 10,
            Name = dto.Name,
            TagType = dto.TagType
        };

        _tagsServiceMock.Setup(s => s.CreateTagAsync(dto, GetCurrentUserId()))
            .ReturnsAsync(created);

        var result = await Controller.Create(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(TagsController.GetById), createdAt.ActionName);
        Assert.Same(created, createdAt.Value);
    }


    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenServiceFails()
    {
        SetupController();

        _tagsServiceMock.Setup(s => s.CreateTagAsync(It.IsAny<NewTagDto>(), GetCurrentUserId()))
            .ReturnsAsync((Tag?)null);

        var dto = new NewTagDto
        {
            Name = "Saint",
            TagType = TagType.Saint
        };

        var result = await Controller.Create(dto);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetupController(authenticated: false);

        var dto = new NewTagDto
        {
            Name = "Saint",
            TagType = TagType.Saint
        };

        var result = await Controller.Create(dto);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        SetupController();

        _tagsServiceMock.Setup(s =>
                s.UpdateTagAsync(
                    1,
                    It.IsAny<NewTagDto>(),
                    GetCurrentUserId()
                ))
            .ReturnsAsync(true);

        var dto = new NewTagDto
        {
            Name = "Saint",
            TagType = TagType.Saint
        };

        var result = await Controller.Update(1, dto);

        AssertNoContent(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _tagsServiceMock.Setup(s =>
                s.UpdateTagAsync(
                    1,
                    It.IsAny<NewTagDto>(),
                    GetCurrentUserId()
                ))
            .ReturnsAsync(false);

        var dto = new NewTagDto
        {
            Name = "Saint",
            TagType = TagType.Saint
        };

        var result = await Controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccessful()
    {
        SetupController();

        _tagsServiceMock.Setup(s => s.DeleteTagAsync(1, GetCurrentUserId()))
            .ReturnsAsync(true);

        var result = await Controller.Delete(1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        SetupController();

        _tagsServiceMock.Setup(s => s.DeleteTagAsync(1, GetCurrentUserId()))
            .ReturnsAsync(false);

        var result = await Controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
