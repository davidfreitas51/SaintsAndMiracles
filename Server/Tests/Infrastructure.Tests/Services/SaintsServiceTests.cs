using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Infrastructure.Tests.Services;

public class SaintsServiceTests
{
    private SaintsService CreateService(
        out Mock<ISaintsRepository> saintsRepo,
        out Mock<ITagsRepository> tagsRepo,
        out Mock<IReligiousOrdersRepository> ordersRepo,
        out Mock<IRecentActivityRepository> activityRepo,
        out Mock<UserManager<AppUser>> userManagerMock,
        string? tempRoot = null)
    {
        saintsRepo = new Mock<ISaintsRepository>();
        tagsRepo = new Mock<ITagsRepository>();
        ordersRepo = new Mock<IReligiousOrdersRepository>();
        activityRepo = new Mock<IRecentActivityRepository>();

        var userStore = new Mock<IUserStore<AppUser>>();
        userManagerMock = new Mock<UserManager<AppUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        var envMock = new Mock<IHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath)
            .Returns(tempRoot ?? Path.GetTempPath());

        return new SaintsService(
            envMock.Object,
            saintsRepo.Object,
            tagsRepo.Object,
            ordersRepo.Object,
            activityRepo.Object,
            userManagerMock.Object
        );
    }

    // -------------------- CREATE --------------------

    [Fact]
    public async Task CreateSaintAsync_ShouldReturnId_WhenValid()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out var userManager
        );

        var newSaint = CreateNewSaintDto(tagIds: new List<int> { 1 });

        var user = new AppUser { Id = "user1" };
        userManager.Setup(u => u.FindByIdAsync("user1"))
            .ReturnsAsync(user);

        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        saintsRepo.Setup(r => r.CreateAsync(It.IsAny<Saint>()))
            .ReturnsAsync(true)
            .Callback<Saint>(s => s.Id = 1);

        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Tag>
            {
                new() { Id = 1, Name = "Tag1", TagType = TagType.Saint }
            });

        var id = await service.CreateSaintAsync(newSaint, "user1");

        Assert.NotNull(id);

        saintsRepo.Verify(r =>
            r.CreateAsync(It.Is<Saint>(s =>
                s.Name == newSaint.Name &&
                s.Country == newSaint.Country &&
                s.Century == newSaint.Century
            )),
            Times.Once
        );

        activityRepo.Verify(a =>
            a.LogActivityAsync(
                EntityType.Saint,
                1,
                newSaint.Name,
                ActivityAction.Created,
                "user1"
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateSaintAsync_ShouldReturnNull_WhenSlugExists()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out _,
            out _
        );

        saintsRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await service.CreateSaintAsync(
            CreateNewSaintDto(name: "Existing"),
            "user1"
        );

        Assert.Null(result);

        saintsRepo.Verify(r =>
            r.CreateAsync(It.IsAny<Saint>()),
            Times.Never
        );
    }

    // -------------------- UPDATE --------------------

    [Fact]
    public async Task UpdateSaintAsync_ShouldReturnFalse_WhenSaintNotFound()
    {
        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out _,
            out _
        );

        saintsRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Saint?)null);

        var result = await service.UpdateSaintAsync(
            1,
            CreateNewSaintDto(),
            "user1"
        );

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateSaintAsync_ShouldUpdateFieldsAndLogActivity()
    {
        var service = CreateService(
            out var saintsRepo,
            out var tagsRepo,
            out _,
            out var activityRepo,
            out _
        );

        var existing = new Saint
        {
            Id = 1,
            Name = "Old",
            Country = "OldCountry",
            Century = 4,
            Description = "OldDesc",
            Image = "old.jpg",
            MarkdownPath = "old.md",
            Slug = "old",
            Tags = new List<Tag>()
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        saintsRepo.Setup(r => r.UpdateAsync(existing))
            .ReturnsAsync(true);

        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Tag>
            {
                new() { Id = 1, Name = "Tag1", TagType = TagType.Saint }
            });

        var updated = CreateNewSaintDto(
            name: "New",
            country: "NewCountry",
            century: 5,
            tagIds: new List<int> { 1 }
        );

        var result = await service.UpdateSaintAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal("New", existing.Name);
        Assert.Equal("NewCountry", existing.Country);
        Assert.Equal(5, existing.Century);
        Assert.Single(existing.Tags);

        activityRepo.Verify(a =>
            a.LogActivityAsync(
                EntityType.Saint,
                1,
                "New",
                ActivityAction.Updated,
                "user1"
            ),
            Times.Once
        );
    }

    // -------------------- DELETE --------------------

    [Fact]
    public async Task DeleteSaintAsync_ShouldDeleteFilesAndLogActivity()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempRoot);

        var service = CreateService(
            out var saintsRepo,
            out _,
            out _,
            out var activityRepo,
            out _,
            tempRoot
        );

        var saint = new Saint
        {
            Id = 1,
            Name = "SaintToDelete",
            Slug = "saint-delete",
            Country = "Country",
            Century = 5,
            Description = "Desc",
            Image = "image.jpg",
            MarkdownPath = "content.md"
        };

        saintsRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(saint);

        saintsRepo.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        var saintFolder = Path.Combine(tempRoot, "wwwroot", "saints", saint.Slug);
        Directory.CreateDirectory(saintFolder);

        await service.DeleteSaintAsync(1, "user1");

        Assert.False(Directory.Exists(saintFolder));

        activityRepo.Verify(a =>
            a.LogActivityAsync(
                EntityType.Saint,
                1,
                saint.Name,
                ActivityAction.Deleted,
                "user1"
            ),
            Times.Once
        );

        Directory.Delete(tempRoot, recursive: true);
    }

    // -------------------- HELPERS --------------------

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
