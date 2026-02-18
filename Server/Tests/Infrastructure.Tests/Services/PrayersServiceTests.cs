using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for PrayersService CRUD operations and tag management.
/// </summary>
public class PrayersServiceTests
{
    private PrayersService CreateService(
        out Mock<IPrayersRepository> prayersRepoMock,
        out Mock<IRecentActivityRepository> activityRepoMock,
        out Mock<ITagsRepository> tagsRepoMock,
        out Mock<IFileStorageService> fileStorageMock)
    {
        prayersRepoMock = new Mock<IPrayersRepository>();
        activityRepoMock = new Mock<IRecentActivityRepository>();
        tagsRepoMock = new Mock<ITagsRepository>();
        fileStorageMock = new Mock<IFileStorageService>();

        fileStorageMock.Setup(f => f.GenerateSlug(It.IsAny<string>()))
            .Returns((string title) => title.ToLower().Replace(" ", "-"));

        fileStorageMock.Setup(f => f.SaveFilesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(("/path/markdown.md", "/path/image.jpg"));

        return new PrayersService(
            prayersRepoMock.Object,
            tagsRepoMock.Object,
            activityRepoMock.Object,
            fileStorageMock.Object,
            NullLogger<PrayersService>.Instance
        );
    }

    // ==================== CREATE PRAYER ====================

    [Fact]
    public async Task CreatePrayerAsync_ShouldReturnId_WhenSlugIsUnique()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);
        var newPrayer = new NewPrayerDto
        {
            Title = "My Prayer",
            Description = "Powerful prayer",
            Image = "image.webp",
            MarkdownContent = "# Prayer Content"
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>()))
            .ReturnsAsync(true)
            .Callback<Prayer>(p => p.Id = 1);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var id = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Equal(1, id);
        prayersRepo.Verify(r => r.CreateAsync(It.Is<Prayer>(p => p.Title == "My Prayer")), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 1, "My Prayer", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldReturnNull_WhenSlugAlreadyExists()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out _, out _);
        var newPrayer = new NewPrayerDto
        {
            Title = "Existing Prayer",
            Description = "Already exists",
            Image = "image.webp",
            MarkdownContent = "Content"
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Null(result);
        prayersRepo.Verify(r => r.CreateAsync(It.IsAny<Prayer>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldReturnNull_WhenRepositoryCreationFails()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);
        var newPrayer = new NewPrayerDto
        {
            Title = "Failed Prayer",
            Description = "Won't be created",
            Image = "image.webp",
            MarkdownContent = "Content"
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Null(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldCreateWithoutTags_WhenTagIdsNotProvided()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);
        var newPrayer = new NewPrayerDto
        {
            Title = "Prayer Without Tags",
            Description = "No tags",
            Image = "image.webp",
            MarkdownContent = "Content"
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>()))
            .ReturnsAsync(true)
            .Callback<Prayer>(p => p.Id = 2);

        var result = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Equal(2, result);
        tagsRepo.Verify(r => r.GetByIdsAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldAddSpecifiedTags()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);
        var tagIds = new List<int> { 1, 2, 3 };
        var newPrayer = new NewPrayerDto
        {
            Title = "Tagged Prayer",
            Description = "Has tags",
            Image = "image.webp",
            MarkdownContent = "Content",
            TagIds = tagIds
        };

        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Hope", TagType = TagType.Prayer },
            new() { Id = 2, Name = "Healing", TagType = TagType.Prayer },
            new() { Id = 3, Name = "Protection", TagType = TagType.Prayer }
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>()))
            .ReturnsAsync(true)
            .Callback<Prayer>(p => p.Id = 3);
        tagsRepo.Setup(r => r.GetByIdsAsync(tagIds)).ReturnsAsync(tags);

        var result = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Equal(3, result);
        prayersRepo.Verify(r => r.CreateAsync(It.Is<Prayer>(p => p.Tags.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldCreateForAnonymousUser()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);
        var newPrayer = new NewPrayerDto
        {
            Title = "Anonymous Prayer",
            Description = "No user",
            Image = "image.webp",
            MarkdownContent = "Content"
        };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>()))
            .ReturnsAsync(true)
            .Callback<Prayer>(p => p.Id = 4);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreatePrayerAsync(newPrayer, null);

        Assert.Equal(4, result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 4, "Anonymous Prayer", ActivityAction.Created, null), Times.Once);
    }

    // ==================== UPDATE PRAYER ====================

    [Fact]
    public async Task UpdatePrayerAsync_ShouldReturnFalse_WhenPrayerNotFound()
    {
        var service = CreateService(out var prayersRepo, out _, out _, out _);
        prayersRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Prayer?)null);

        var updated = new NewPrayerDto
        {
            Title = "Updated",
            Description = "New description",
            Image = "image.webp",
            MarkdownContent = "New content"
        };
        var result = await service.UpdatePrayerAsync(999, updated, "user1");

        Assert.False(result);
        prayersRepo.Verify(r => r.UpdateAsync(It.IsAny<Prayer>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldUpdateFieldsAndLogActivity()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Prayer
        {
            Id = 1,
            Title = "Old Title",
            Description = "Old description",
            Tags = new List<Tag>(),
            Image = "old.jpg",
            MarkdownPath = "old.md",
            Slug = "old-title"
        };

        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        prayersRepo.Setup(r => r.UpdateAsync(It.IsAny<Prayer>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var updated = new NewPrayerDto
        {
            Title = "New Title",
            Description = "New description",
            MarkdownContent = "New content",
            Image = "new.jpg"
        };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal("New Title", existing.Title);
        Assert.Equal("New description", existing.Description);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 1, "New Title", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Prayer
        {
            Id = 1,
            Title = "Title",
            Description = "Description",
            Tags = new List<Tag>(),
            Image = "",
            MarkdownPath = "",
            Slug = "title"
        };

        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        prayersRepo.Setup(r => r.UpdateAsync(It.IsAny<Prayer>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var updated = new NewPrayerDto
        {
            Title = "New Title",
            Description = "New description",
            MarkdownContent = "Content",
            Image = "image.webp"
        };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldReplaceTags()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Prayer
        {
            Id = 1,
            Title = "Prayer",
            Description = "Description",
            Tags = new List<Tag> { new() { Id = 1, Name = "Old", TagType = TagType.Prayer } },
            Image = "",
            MarkdownPath = "",
            Slug = "prayer"
        };

        var newTags = new List<Tag>
        {
            new() { Id = 5, Name = "Hope", TagType = TagType.Prayer },
            new() { Id = 6, Name = "Healing", TagType = TagType.Prayer }
        };

        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        prayersRepo.Setup(r => r.UpdateAsync(It.IsAny<Prayer>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(new List<int> { 5, 6 })).ReturnsAsync(newTags);

        var updated = new NewPrayerDto
        {
            Title = "Prayer",
            Description = "Description",
            MarkdownContent = "Content",
            Image = "image.webp",
            TagIds = new List<int> { 5, 6 }
        };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal(2, existing.Tags.Count);
        Assert.Equal("Hope", existing.Tags[0].Name);
        Assert.Equal("Healing", existing.Tags[1].Name);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldRemoveTags_WhenEmptyTagIds()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Prayer
        {
            Id = 1,
            Title = "Prayer",
            Description = "Description",
            Tags = new List<Tag> { new() { Id = 1, Name = "OldTag", TagType = TagType.Prayer } },
            Image = "",
            MarkdownPath = "",
            Slug = "prayer"
        };

        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        prayersRepo.Setup(r => r.UpdateAsync(It.IsAny<Prayer>())).ReturnsAsync(true);

        var updated = new NewPrayerDto
        {
            Title = "Prayer",
            Description = "Description",
            MarkdownContent = "Content",
            Image = "image.webp"
        };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Empty(existing.Tags);
    }

    // ==================== DELETE PRAYER ====================

    [Fact]
    public async Task DeletePrayerAsync_ShouldNotThrow_WhenPrayerNotFound()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out _, out var fileStorage);
        prayersRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Prayer?)null);

        // Should not throw
        await service.DeletePrayerAsync("nonexistent-slug", "user1");

        prayersRepo.Verify(r => r.DeleteAsync(It.IsAny<Prayer>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeletePrayerAsync_ShouldDeleteFilesAndPrayerAndLogActivity()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out _, out var fileStorage);

        var prayer = new Prayer
        {
            Id = 1,
            Title = "Prayer to Delete",
            Description = "Will be deleted",
            Tags = new List<Tag>(),
            Image = "delete-me.jpg",
            MarkdownPath = "delete-me.md",
            Slug = "prayer-to-delete"
        };

        prayersRepo.Setup(r => r.GetBySlugAsync("prayer-to-delete")).ReturnsAsync(prayer);
        fileStorage.Setup(f => f.DeleteFolderAsync("prayers", "prayer-to-delete")).Returns(Task.CompletedTask);
        prayersRepo.Setup(r => r.DeleteAsync(prayer)).ReturnsAsync(true);

        await service.DeletePrayerAsync("prayer-to-delete", "user1");

        fileStorage.Verify(f => f.DeleteFolderAsync("prayers", "prayer-to-delete"), Times.Once);
        prayersRepo.Verify(r => r.DeleteAsync(prayer), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 1, "Prayer to Delete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeletePrayerAsync_ShouldDeleteForAnonymousUser()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out _, out var fileStorage);

        var prayer = new Prayer
        {
            Id = 5,
            Title = "Anon Prayer",
            Description = "No user",
            Tags = new List<Tag>(),
            Image = "",
            MarkdownPath = "",
            Slug = "anon-prayer"
        };

        prayersRepo.Setup(r => r.GetBySlugAsync("anon-prayer")).ReturnsAsync(prayer);
        fileStorage.Setup(f => f.DeleteFolderAsync("prayers", "anon-prayer")).Returns(Task.CompletedTask);
        prayersRepo.Setup(r => r.DeleteAsync(prayer)).ReturnsAsync(true);

        await service.DeletePrayerAsync("anon-prayer", null);

        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 5, "Anon Prayer", ActivityAction.Deleted, null), Times.Once);
    }
}
