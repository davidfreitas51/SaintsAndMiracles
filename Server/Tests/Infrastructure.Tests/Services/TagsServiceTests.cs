using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for TagsService CRUD operations and tag type management.
/// </summary>
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
            activityRepo.Object,
            NullLogger<TagsService>.Instance
        );
    }

    // ==================== CREATE TAG ====================

    [Fact]
    public async Task CreateTagAsync_ShouldReturnTag_WhenCreationSucceeds()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "Healing", TagType = TagType.Miracle };
        var createdId = 0;

        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(true)
            .Callback<Tag>(t => { t.Id = 1; createdId = 1; });

        var result = await service.CreateTagAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Healing", result.Name);
        Assert.Equal(TagType.Miracle, result.TagType);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "Healing", ActivityAction.Created, "user1"), Times.Once);
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
    public async Task CreateTagAsync_ShouldCreateSaintTag()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "Martyr", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(true)
            .Callback<Tag>(t => t.Id = 2);

        var result = await service.CreateTagAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(TagType.Saint, result!.TagType);
        Assert.Equal("Martyr", result.Name);
    }

    [Fact]
    public async Task CreateTagAsync_ShouldCreatePrayerTag()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "Intercession", TagType = TagType.Prayer };
        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(true)
            .Callback<Tag>(t => t.Id = 3);

        var result = await service.CreateTagAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(TagType.Prayer, result!.TagType);
        Assert.Equal("Intercession", result.Name);
    }

    [Fact]
    public async Task CreateTagAsync_ShouldCreateForAnonymousUser()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var dto = new NewTagDto { Name = "AnonTag", TagType = TagType.Miracle };
        tagsRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(true)
            .Callback<Tag>(t => t.Id = 4);

        var result = await service.CreateTagAsync(dto, null);

        Assert.NotNull(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 4, "AnonTag", ActivityAction.Created, null), Times.Once);
    }

    // ==================== UPDATE TAG ====================

    [Fact]
    public async Task UpdateTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        tagsRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tag?)null);

        var dto = new NewTagDto { Name = "Updated", TagType = TagType.Saint };
        var result = await service.UpdateTagAsync(999, dto, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldUpdateTagAndLogActivity()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 1, Name = "OldName", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewTagDto { Name = "NewName", TagType = TagType.Miracle };
        var result = await service.UpdateTagAsync(1, dto, "user1");

        Assert.True(result);
        Assert.Equal("NewName", existing.Name);
        Assert.Equal(TagType.Miracle, existing.TagType);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "NewName", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 1, Name = "Original", TagType = TagType.Prayer };
        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(false);

        var dto = new NewTagDto { Name = "NewName", TagType = TagType.Saint };
        var result = await service.UpdateTagAsync(1, dto, "user1");

        // Service modifies object before calling UpdateAsync, so object will be changed even if update fails
        // What matters is that UpdateAsync returns false and activity is not logged
        Assert.False(result);
        Assert.Equal("NewName", existing.Name); // Object was modified by service
        Assert.Equal(TagType.Saint, existing.TagType); // Object was modified by service
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldUpdateName_OnlyNameChange()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 1, Name = "Old", TagType = TagType.Saint };
        tagsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewTagDto { Name = "NewName", TagType = TagType.Saint };
        var result = await service.UpdateTagAsync(1, dto, "user1");

        Assert.True(result);
        Assert.Equal("NewName", existing.Name);
        Assert.Equal(TagType.Saint, existing.TagType); // Should not change
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "NewName", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdateTagAsync_ShouldUpdateForAnonymousUser()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 5, Name = "OldAnon", TagType = TagType.Miracle };
        tagsRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewTagDto { Name = "NewAnon", TagType = TagType.Prayer };
        var result = await service.UpdateTagAsync(5, dto, null);

        Assert.True(result);
        Assert.Equal("NewAnon", existing.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 5, "NewAnon", ActivityAction.Updated, null), Times.Once);
    }

    // ==================== DELETE TAG ====================

    [Fact]
    public async Task DeleteTagAsync_ShouldReturnFalse_WhenTagNotFound()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        tagsRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tag?)null);

        var result = await service.DeleteTagAsync(999, "user1");

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
        tagsRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 1, "TagToDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldDeleteMiracleTag()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 2, Name = "MiracleDelete", TagType = TagType.Miracle };
        tagsRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.DeleteAsync(2)).Returns(Task.CompletedTask);

        var result = await service.DeleteTagAsync(2, "user1");

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 2, "MiracleDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldDeletePrayerTag()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 3, Name = "PrayerDelete", TagType = TagType.Prayer };
        tagsRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.DeleteAsync(3)).Returns(Task.CompletedTask);

        var result = await service.DeleteTagAsync(3, "user1");

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 3, "PrayerDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_ShouldDeleteForAnonymousUser()
    {
        var service = CreateService(out var tagsRepo, out var activityRepo);

        var existing = new Tag { Id = 10, Name = "AnonDelete", TagType = TagType.Miracle };
        tagsRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existing);
        tagsRepo.Setup(r => r.DeleteAsync(10)).Returns(Task.CompletedTask);

        var result = await service.DeleteTagAsync(10, null);

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Tag, 10, "AnonDelete", ActivityAction.Deleted, null), Times.Once);
    }
}
