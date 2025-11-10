using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class TagsServiceTests
{
    private TagsService CreateService(
        out Mock<ITagsRepository> tagsRepo,
        out Mock<IRecentActivityRepository> activityRepo)
    {
        tagsRepo = new Mock<ITagsRepository>();
        activityRepo = new Mock<IRecentActivityRepository>();

        return new TagsService(
            tagsRepo.Object,
            activityRepo.Object
        );
    }

    [Fact]
    public async Task CreateTagAsync_ShouldReturnTagAndLogActivity()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "Tag1", TagType = TagType.Saint };
        var tag = new Tag { Id = 1, Name = dto.Name, TagType = dto.TagType };
        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>())).ReturnsAsync(true)
                .Callback<Tag>(t => t.Id = 1);

        var result = await service.CreateTagAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Tag1", result.Name);
        Assert.Equal(TagType.Saint, result.TagType);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "Tag1", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreateTagAsync_ShouldReturnNull_WhenCreationFails()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "FailTag", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>())).ReturnsAsync(false);

        var result = await service.CreateTagAsync(dto, "user1");

        Assert.Null(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        var dto = new NewTagDto { Name = "Updated", TagType = TagType.Saint };
        var result = await service.UpdateTagAsync(1, dto, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldUpdateAndLogActivity()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 1, Name = "Old", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewTagDto { Name = "New", TagType = TagType.Miracle };
        var result = await service.UpdateTagAsync(1, dto, "user1");

        Assert.True(result);
        Assert.Equal("New", existing.Name);
        Assert.Equal(TagType.Miracle, existing.TagType);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "New", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Tag?)null);

        var result = await service.DeleteTagAsync(1, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldDeleteAndLogActivity()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 1, Name = "TagToDelete", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        var result = await service.DeleteTagAsync(1, "user1");

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "TagToDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

}
