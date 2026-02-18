using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for SaintsService CRUD operations and saint management with religious orders.
/// </summary>
public class SaintsServiceTests
{
    private SaintsService CreateService(
        out Mock<ISaintsRepository> saintsRepo,
        out Mock<ITagsRepository> tagsRepo,
        out Mock<IReligiousOrdersRepository> ordersRepo,
        out Mock<IRecentActivityRepository> activityRepo,
        out Mock<UserManager<AppUser>> userManagerMock,
        out Mock<IFileStorageService> fileStorageMock)
    {
        saintsRepo = new Mock<ISaintsRepository>();
        tagsRepo = new Mock<ITagsRepository>();
        ordersRepo = new Mock<IReligiousOrdersRepository>();
        activityRepo = new Mock<IRecentActivityRepository>();
        fileStorageMock = new Mock<IFileStorageService>();

        var userStore = new Mock<IUserStore<AppUser>>();
        userManagerMock = new Mock<UserManager<AppUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        fileStorageMock.Setup(f => f.GenerateSlug(It.IsAny<string>()))
            .Returns((string name) => name.ToLower().Replace(" ", "-"));

        fileStorageMock.Setup(f => f.SaveFilesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(("/path/markdown.md", "/path/image.jpg"));

        return new SaintsService(
            saintsRepo.Object,
            tagsRepo.Object,
            ordersRepo.Object,
            activityRepo.Object,
            userManagerMock.Object,
            fileStorageMock.Object,
            NullLogger<SaintsService>.Instance
        );
    }

    // ==================== CREATE SAINT ====================

    [Fact]
    public async Task CreateSaintAsync_ShouldReturnId_WhenSlugIsUnique()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out var ordersRepo,
            out var activityRepo,
            out var userManager,
            out _
        );

        var newSaint = CreateNewSaintDto(name: "Saint Francis", country: "Italy", century: 13, tagIds: new List<int> { 1 });
        var user = new AppUser { Id = "user1" };

        userManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(user);
        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>()))
            .ReturnsAsync(true)
            .Callback<Saint>(s => s.Id = 1);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Tag> { new() { Id = 1, Name = "Friar", TagType = TagType.Saint } });

        var id = await service.CreateSaintAsync(newSaint, "user1");

        Assert.Equal(1, id);
        saintsRepo.Verify(r => r.CreateAsync(It.Is<Saint>(s =>
            s.Name == "Saint Francis" &&
            s.Country == "Italy" &&
            s.Century == 13)), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Saint, 1, "Saint Francis", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldReturnNull_WhenSlugAlreadyExists()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out var activityRepo,
            out var userManager,
            out _
        );

        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await service.CreateSaintAsync(CreateNewSaintDto(name: "Existing"), "user1");

        Assert.Null(result);
        saintsRepo.Verify(r => r.CreateAsync(It.IsAny<Saint>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldReturnNull_WhenRepositoryCreationFails()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out var userManager,
            out _
        );

        var newSaint = CreateNewSaintDto();
        userManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(new AppUser { Id = "user1" });
        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreateSaintAsync(newSaint, "user1");

        Assert.Null(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldCreateWithoutTags_WhenTagIdsNotProvided()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out var userManager,
            out _
        );

        var newSaint = CreateNewSaintDto(tagIds: null);
        userManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(new AppUser { Id = "user1" });
        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>()))
            .ReturnsAsync(true)
            .Callback<Saint>(s => s.Id = 2);

        var result = await service.CreateSaintAsync(newSaint, "user1");

        Assert.Equal(2, result);
        tagsRepo.Verify(r => r.GetByIdsAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldAddMultipleTags()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out var userManager,
            out _
        );

        var tagIds = new List<int> { 1, 2, 3 };
        var newSaint = CreateNewSaintDto(tagIds: tagIds);
        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "Martyr", TagType = TagType.Saint },
            new() { Id = 2, Name = "Virgin", TagType = TagType.Saint },
            new() { Id = 3, Name = "Doctor", TagType = TagType.Saint }
        };

        userManager.Setup(u => u.FindByIdAsync("user1")).ReturnsAsync(new AppUser { Id = "user1" });
        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>()))
            .ReturnsAsync(true)
            .Callback<Saint>(s => s.Id = 3);
        tagsRepo.Setup(r => r.GetByIdsAsync(tagIds)).ReturnsAsync(tags);

        var result = await service.CreateSaintAsync(newSaint, "user1");

        Assert.Equal(3, result);
        saintsRepo.Verify(r => r.CreateAsync(It.Is<Saint>(s => s.Tags.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldCreateForAnonymousUser()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out var userManager,
            out _
        );

        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>()))
            .ReturnsAsync(true)
            .Callback<Saint>(s => s.Id = 4);
        userManager.Setup(u => u.FindByIdAsync(null)).ReturnsAsync((AppUser?)null);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.CreateSaintAsync(CreateNewSaintDto(), null);

        Assert.Null(result); // Anonymous users cannot create saints - user must be found
    }

    // ==================== UPDATE SAINT ====================

    [Fact]
    public async Task UpdateSaintAsync_ShouldReturnFalse_WhenSaintNotFound()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out _,
            out _,
            out _
        );

        saintsRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Saint?)null);

        var result = await service.UpdateSaintAsync(999, CreateNewSaintDto(), "user1");

        Assert.False(result);
        saintsRepo.Verify(r => r.UpdateAsync(It.IsAny<Saint>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSaintAsync_ShouldUpdateAllFieldsAndLogActivity()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out _,
            out _
        );

        var existing = new Saint
        {
            Id = 1,
            Name = "Old Name",
            Country = "Spain",
            Century = 12,
            Description = "Old description",
            Image = "old.jpg",
            MarkdownPath = "old.md",
            Slug = "old-name",
            Tags = new List<Tag>()
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        saintsRepo.Setup(r => r.UpdateAsync(It.IsAny<Saint>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var updated = CreateNewSaintDto(name: "New Name", country: "Italy", century: 13);
        var result = await service.UpdateSaintAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal("New Name", existing.Name);
        Assert.Equal("Italy", existing.Country);
        Assert.Equal(13, existing.Century);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Saint, 1, "New Name", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdateSaintAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out _,
            out _
        );

        var existing = new Saint
        {
            Id = 1,
            Name = "Saint",
            Country = "France",
            Century = 15,
            Description = "Description",
            Image = "",
            MarkdownPath = "",
            Slug = "saint",
            Tags = new List<Tag>()
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        saintsRepo.Setup(r => r.UpdateAsync(It.IsAny<Saint>())).ReturnsAsync(false);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var result = await service.UpdateSaintAsync(1, CreateNewSaintDto(name: "New"), "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSaintAsync_ShouldReplaceTags()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out _,
            out _
        );

        var existing = new Saint
        {
            Id = 1,
            Name = "Saint",
            Country = "Italy",
            Century = 13,
            Description = "Description",
            Image = "",
            MarkdownPath = "",
            Slug = "saint",
            Tags = new List<Tag> { new() { Id = 1, Name = "OldTag", TagType = TagType.Saint } }
        };

        var newTags = new List<Tag>
        {
            new() { Id = 5, Name = "Martyr", TagType = TagType.Saint },
            new() { Id = 6, Name = "Virgin", TagType = TagType.Saint }
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        saintsRepo.Setup(r => r.UpdateAsync(It.IsAny<Saint>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(new List<int> { 5, 6 })).ReturnsAsync(newTags);

        var updated = CreateNewSaintDto(tagIds: new List<int> { 5, 6 });
        var result = await service.UpdateSaintAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal(2, existing.Tags.Count);
        Assert.Equal("Martyr", existing.Tags[0].Name);
        Assert.Equal("Virgin", existing.Tags[1].Name);
    }

    [Fact]
    public async Task UpdateSaintAsync_ShouldRemoveTags_WhenEmptyTagIds()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out _,
            out _
        );

        var existing = new Saint
        {
            Id = 1,
            Name = "Saint",
            Country = "Italy",
            Century = 13,
            Description = "Description",
            Image = "",
            MarkdownPath = "",
            Slug = "saint",
            Tags = new List<Tag> { new() { Id = 1, Name = "OldTag", TagType = TagType.Saint } }
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        saintsRepo.Setup(r => r.UpdateAsync(It.IsAny<Saint>())).ReturnsAsync(true);

        var updated = CreateNewSaintDto(tagIds: null);
        var result = await service.UpdateSaintAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Empty(existing.Tags);
    }

    // ==================== DELETE SAINT ====================

    [Fact]
    public async Task DeleteSaintAsync_ShouldNotThrow_WhenSaintNotFound()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out var activityRepo,
            out _,
            out var fileStorage
        );

        saintsRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Saint?)null);

        // Should not throw
        await service.DeleteSaintAsync(999, "user1");

        saintsRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSaintAsync_ShouldDeleteFilesAndSaintAndLogActivity()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out var activityRepo,
            out _,
            out var fileStorage
        );

        var saint = new Saint
        {
            Id = 1,
            Name = "Saint to Delete",
            Country = "Italy",
            Century = 13,
            Description = "Will be deleted",
            Image = "delete.jpg",
            MarkdownPath = "delete.md",
            Slug = "saint-to-delete",
            Tags = new List<Tag>()
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(saint);
        fileStorage.Setup(f => f.DeleteFolderAsync("saints", "saint-to-delete")).Returns(Task.CompletedTask);
        saintsRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        await service.DeleteSaintAsync(1, "user1");

        fileStorage.Verify(f => f.DeleteFolderAsync("saints", "saint-to-delete"), Times.Once);
        saintsRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Saint, 1, "Saint to Delete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteSaintAsync_ShouldDeleteForAnonymousUser()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out var activityRepo,
            out _,
            out var fileStorage
        );

        var saint = new Saint
        {
            Id = 5,
            Name = "Anon Saint",
            Country = "France",
            Century = 10,
            Description = "No user",
            Image = "",
            MarkdownPath = "",
            Slug = "anon-saint",
            Tags = new List<Tag>()
        };

        saintsRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(saint);
        fileStorage.Setup(f => f.DeleteFolderAsync("saints", "anon-saint")).Returns(Task.CompletedTask);
        saintsRepo.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);

        await service.DeleteSaintAsync(5, null);

        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Saint, 5, "Anon Saint", ActivityAction.Deleted, null), Times.Once);
    }

    // ==================== HELPERS ====================

    private static NewSaintDto CreateNewSaintDto(
        string? name = null,
        string? country = null,
        int century = 5,
        string? image = null,
        string? description = null,
        string? markdown = null,
        List<int>? tagIds = null)
    {
        return new NewSaintDto
        {
            Name = name ?? "Saint Test",
            Country = country ?? "Italy",
            Century = century,
            Image = image ?? "saint.jpg",
            Description = description ?? "Saint description",
            MarkdownContent = markdown ?? "saint.md",
            TagIds = tagIds
        };
    }
}
