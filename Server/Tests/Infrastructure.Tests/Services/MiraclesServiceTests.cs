using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for MiraclesService CRUD operations and tag management.
/// </summary>
public class MiraclesServiceTests
{
    private MiraclesService CreateService(
        out Mock<IMiraclesRepository> miraclesRepoMock,
        out Mock<IRecentActivityRepository> activityRepoMock,
        out Mock<ITagsRepository> tagsRepoMock,
        out Mock<IFileStorageService> fileStorageMock)
    {
        miraclesRepoMock = new Mock<IMiraclesRepository>();
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

        return new MiraclesService(
            miraclesRepoMock.Object,
            tagsRepoMock.Object,
            activityRepoMock.Object,
            fileStorageMock.Object,
            NullLogger<MiraclesService>.Instance
        );
    }

    // ==================== CREATE MIRACLE ====================

    [Fact]
    public async Task CreateMiracleAsync_ShouldReturnId_WhenSlugIsUnique()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var newMiracle = new NewMiracleDto
        {
            Title = "Miracle of Lourdes",
            Country = "France",
            Century = 19,
            Description = "Miraculous healing",
            Image = "image.webp",
            MarkdownContent = "# Miracle Content",
            Date = "1858-02-11",
            LocationDetails = "Grotto of Massabielle"
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>()))
            .ReturnsAsync(true)
            .Callback<Miracle>(m => m.Id = 1);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var id = await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.Equal(1, id);
        miraclesRepo.Verify(r => r.CreateAsync(It.Is<Miracle>(m =>
            m.Title == "Miracle of Lourdes" &&
            m.Country == "France" &&
            m.Century == 19)), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Miracle, 1, "Miracle of Lourdes", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldReturnNull_WhenSlugAlreadyExists()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out _, out _);
        var newMiracle = new NewMiracleDto
        {
            Title = "Existing Miracle",
            Country = "France",
            Century = 19,
            Description = "Already exists",
            Image = "image.webp",
            MarkdownContent = "Content",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.Null(result);
        miraclesRepo.Verify(r => r.CreateAsync(It.IsAny<Miracle>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldReturnNull_WhenRepositoryCreationFails()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var newMiracle = new NewMiracleDto
        {
            Title = "Failed Miracle",
            Country = "Italy",
            Century = 20,
            Description = "Won't be created",
            Image = "image.webp",
            MarkdownContent = "Content",
            Date = "2000-01-01",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.Null(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldCreateWithoutTags_WhenTagIdsNotProvided()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var newMiracle = new NewMiracleDto
        {
            Title = "Untagged Miracle",
            Country = "Spain",
            Century = 18,
            Description = "No tags",
            Image = "image.webp",
            MarkdownContent = "Content",
            Date = "1700-01-01",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>()))
            .ReturnsAsync(true)
            .Callback<Miracle>(m => m.Id = 2);

        var result = await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.Equal(2, result);
        tagsRepo.Verify(r => r.GetByIdsAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldAddSpecifiedTags()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var tagIds = new List<int> { 1, 2 };
        var newMiracle = new NewMiracleDto
        {
            Title = "Tagged Miracle",
            Country = "Portugal",
            Century = 17,
            Description = "Has tags",
            Image = "image.webp",
            MarkdownContent = "Content",
            TagIds = tagIds,
            Date = "1600-01-01",
            LocationDetails = "Location"
        };

        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Healing", TagType = TagType.Miracle },
            new() { Id = 2, Name = "Witness", TagType = TagType.Miracle }
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>()))
            .ReturnsAsync(true)
            .Callback<Miracle>(m => m.Id = 3);
        tagsRepo.Setup(r => r.GetByIdsAsync(tagIds)).ReturnsAsync(tags);

        var result = await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.Equal(3, result);
        miraclesRepo.Verify(r => r.CreateAsync(It.Is<Miracle>(m => m.Tags.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldCreateForAnonymousUser()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var newMiracle = new NewMiracleDto
        {
            Title = "Anonymous Miracle",
            Country = "Germany",
            Century = 16,
            Description = "No user",
            Image = "image.webp",
            MarkdownContent = "Content",
            Date = "1500-01-01",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>()))
            .ReturnsAsync(true)
            .Callback<Miracle>(m => m.Id = 4);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreateMiracleAsync(newMiracle, null);

        Assert.Equal(4, result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Miracle, 4, "Anonymous Miracle", ActivityAction.Created, null), Times.Once);
    }

    // ==================== UPDATE MIRACLE ====================

    [Fact]
    public async Task UpdateMiracleAsync_ShouldReturnFalse_WhenMiracleNotFound()
    {
        var service = CreateService(out var miraclesRepo, out _, out _, out _);
        miraclesRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Miracle?)null);

        var updated = new NewMiracleDto
        {
            Title = "Updated",
            Country = "France",
            Century = 19,
            Description = "New description",
            Image = "image.webp",
            MarkdownContent = "New content",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };
        var result = await service.UpdateMiracleAsync(999, updated, "user1");

        Assert.False(result);
        miraclesRepo.Verify(r => r.UpdateAsync(It.IsAny<Miracle>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMiracleAsync_ShouldUpdateAllFields()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Miracle
        {
            Id = 1,
            Title = "Old Title",
            Country = "Spain",
            Century = 15,
            Description = "Old description",
            Tags = new List<Tag>(),
            Image = "old.jpg",
            MarkdownPath = "old.md",
            Slug = "old-title",
            Date = "1500-01-01",
            LocationDetails = "Old Location"
        };

        miraclesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        miraclesRepo.Setup(r => r.UpdateAsync(It.IsAny<Miracle>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var updated = new NewMiracleDto
        {
            Title = "New Title",
            Country = "France",
            Century = 19,
            Description = "New description",
            MarkdownContent = "New content",
            Image = "new.jpg",
            Date = "1858-02-11",
            LocationDetails = "New Location"
        };
        var result = await service.UpdateMiracleAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal("New Title", existing.Title);
        Assert.Equal("France", existing.Country);
        Assert.Equal(19, existing.Century);
        Assert.Equal("New description", existing.Description);
        Assert.Equal("1858-02-11", existing.Date);
        Assert.Equal("New Location", existing.LocationDetails);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Miracle, 1, "New Title", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdateMiracleAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Miracle
        {
            Id = 1,
            Title = "Title",
            Country = "Italy",
            Century = 20,
            Description = "Description",
            Tags = new List<Tag>(),
            Image = "",
            MarkdownPath = "",
            Slug = "title",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        miraclesRepo.Setup(r => r.UpdateAsync(It.IsAny<Miracle>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var updated = new NewMiracleDto
        {
            Title = "New Title",
            Country = "Germany",
            Century = 19,
            Description = "New description",
            MarkdownContent = "Content",
            Image = "image.webp",
            Date = "1859-02-11",
            LocationDetails = "New Location"
        };
        var result = await service.UpdateMiracleAsync(1, updated, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMiracleAsync_ShouldReplaceTags()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Miracle
        {
            Id = 1,
            Title = "Miracle",
            Country = "France",
            Century = 19,
            Description = "Description",
            Tags = new List<Tag> { new() { Id = 1, Name = "Old", TagType = TagType.Miracle } },
            Image = "",
            MarkdownPath = "",
            Slug = "miracle",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        var newTags = new List<Tag>
        {
            new() { Id = 5, Name = "Healing", TagType = TagType.Miracle },
            new() { Id = 6, Name = "Saint", TagType = TagType.Miracle }
        };

        miraclesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        miraclesRepo.Setup(r => r.UpdateAsync(It.IsAny<Miracle>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(new List<int> { 5, 6 })).ReturnsAsync(newTags);

        var updated = new NewMiracleDto
        {
            Title = "Miracle",
            Country = "France",
            Century = 19,
            Description = "Description",
            MarkdownContent = "Content",
            Image = "image.webp",
            TagIds = new List<int> { 5, 6 },
            Date = "1858-02-11",
            LocationDetails = "Location"
        };
        var result = await service.UpdateMiracleAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal(2, existing.Tags.Count);
        Assert.Equal("Healing", existing.Tags[0].Name);
        Assert.Equal("Saint", existing.Tags[1].Name);
    }

    [Fact]
    public async Task UpdateMiracleAsync_ShouldRemoveTags_WhenEmptyTagIds()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);

        var existing = new Miracle
        {
            Id = 1,
            Title = "Miracle",
            Country = "France",
            Century = 19,
            Description = "Description",
            Tags = new List<Tag> { new() { Id = 1, Name = "OldTag", TagType = TagType.Miracle } },
            Image = "",
            MarkdownPath = "",
            Slug = "miracle",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        miraclesRepo.Setup(r => r.UpdateAsync(It.IsAny<Miracle>())).ReturnsAsync(true);

        var updated = new NewMiracleDto
        {
            Title = "Miracle",
            Country = "France",
            Century = 19,
            Description = "Description",
            MarkdownContent = "Content",
            Image = "image.webp",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };
        var result = await service.UpdateMiracleAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Empty(existing.Tags);
    }

    // ==================== DELETE MIRACLE ====================

    [Fact]
    public async Task DeleteMiracleAsync_ShouldNotThrow_WhenMiracleNotFound()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out _, out var fileStorage);
        miraclesRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Miracle?)null);

        // Should not throw
        await service.DeleteMiracleAsync("nonexistent-slug", "user1");

        miraclesRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMiracleAsync_ShouldDeleteFilesAndMiracleAndLogActivity()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out _, out var fileStorage);

        var miracle = new Miracle
        {
            Id = 1,
            Title = "Miracle to Delete",
            Country = "France",
            Century = 19,
            Description = "Will be deleted",
            Tags = new List<Tag>(),
            Image = "delete-me.jpg",
            MarkdownPath = "delete-me.md",
            Slug = "miracle-to-delete",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.GetBySlugAsync("miracle-to-delete")).ReturnsAsync(miracle);
        fileStorage.Setup(f => f.DeleteFolderAsync("miracles", "miracle-to-delete")).Returns(Task.CompletedTask);
        miraclesRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        await service.DeleteMiracleAsync("miracle-to-delete", "user1");

        fileStorage.Verify(f => f.DeleteFolderAsync("miracles", "miracle-to-delete"), Times.Once);
        miraclesRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Miracle, 1, "Miracle to Delete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteMiracleAsync_ShouldDeleteForAnonymousUser()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out _, out var fileStorage);

        var miracle = new Miracle
        {
            Id = 5,
            Title = "Anon Miracle",
            Country = "Spain",
            Century = 18,
            Description = "No user",
            Tags = new List<Tag>(),
            Image = "",
            MarkdownPath = "",
            Slug = "anon-miracle",
            Date = "1858-02-11",
            LocationDetails = "Location"
        };

        miraclesRepo.Setup(r => r.GetBySlugAsync("anon-miracle")).ReturnsAsync(miracle);
        fileStorage.Setup(f => f.DeleteFolderAsync("miracles", "anon-miracle")).Returns(Task.CompletedTask);
        miraclesRepo.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

        await service.DeleteMiracleAsync("anon-miracle", null);

        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Miracle, 5, "Anon Miracle", ActivityAction.Deleted, null), Times.Once);
    }

    [Fact]
    public async Task CreateMiracleAsync_ShouldPreserveAllMiracleProperties()
    {
        var service = CreateService(out var miraclesRepo, out var activityRepo, out var tagsRepo, out _);
        var date = "1858-02-11";
        var newMiracle = new NewMiracleDto
        {
            Title = "Full Miracle",
            Country = "France",
            Century = 19,
            Description = "Complete details",
            Image = "miracle.jpg",
            MarkdownContent = "# The Miracle",
            Date = date,
            LocationDetails = "Lourdes Grotto"
        };

        Miracle? capturedMiracle = null;
        miraclesRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        miraclesRepo.Setup(r => r.CreateAsync(It.IsAny<Miracle>()))
            .ReturnsAsync(true)
            .Callback<Miracle>(m =>
            {
                m.Id = 10;
                capturedMiracle = m;
            });
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        await service.CreateMiracleAsync(newMiracle, "user1");

        Assert.NotNull(capturedMiracle);
        Assert.Equal("Full Miracle", capturedMiracle.Title);
        Assert.Equal("France", capturedMiracle.Country);
        Assert.Equal(19, capturedMiracle.Century);
        Assert.Equal("Complete details", capturedMiracle.Description);
        Assert.Equal(date, capturedMiracle.Date);
        Assert.Equal("Lourdes Grotto", capturedMiracle.LocationDetails);
    }
}
