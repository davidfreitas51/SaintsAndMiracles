using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Infrastructure.Tests.Services;

public class PrayersServiceTests
{
    private PrayersService CreateService(
        out Mock<IPrayersRepository> prayersRepoMock,
        out Mock<IRecentActivityRepository> activityRepoMock,
        out Mock<ITagsRepository> tagsRepoMock,
        string? tempRoot = null)
    {
        prayersRepoMock = new Mock<IPrayersRepository>();
        activityRepoMock = new Mock<IRecentActivityRepository>();
        tagsRepoMock = new Mock<ITagsRepository>();

        var envMock = new Mock<IHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(tempRoot ?? Path.GetTempPath());

        var fileStorageMock = new Mock<IFileStorageService>();

        return new PrayersService(
            prayersRepoMock.Object,
            tagsRepoMock.Object,
            activityRepoMock.Object,
            fileStorageMock.Object
        );
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldReturnId_WhenSlugIsUnique()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo);
        var newPrayer = new NewPrayerDto { Title = "My Prayer", Description = "Desc", Image="image.webp", MarkdownContent = "Content" };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        prayersRepo.Setup(r => r.CreateAsync(It.IsAny<Prayer>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Tag>());

        var id = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.NotNull(id);
        prayersRepo.Verify(r => r.CreateAsync(It.Is<Prayer>(p => p.Title == "My Prayer")), Times.Once);
        activityRepo.Verify(r => r.LogActivityAsync(EntityType.Prayer, It.IsAny<int>(), "My Prayer", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreatePrayerAsync_ShouldReturnNull_WhenSlugExists()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo);
        var newPrayer = new NewPrayerDto { Title = "Existing", Description = "", Image="image.webp", MarkdownContent = "" };

        prayersRepo.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await service.CreatePrayerAsync(newPrayer, "user1");

        Assert.Null(result);
        prayersRepo.Verify(r => r.CreateAsync(It.IsAny<Prayer>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldReturnFalse_WhenPrayerNotFound()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo);
        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Prayer?)null);

        var updated = new NewPrayerDto { Title = "Updated", Description = "Desc", Image="image.webp", MarkdownContent = "Content" };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePrayerAsync_ShouldUpdateFieldsAndLogActivity()
    {
        var service = CreateService(out var prayersRepo, out var activityRepo, out var tagsRepo);

        var existing = new Prayer
        {
            Id = 1,
            Title = "Old",
            Description = "OldDesc",
            Tags = new List<Tag>(),
            Image = "",
            MarkdownPath = "",
            Slug = "old"
        };
        prayersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        prayersRepo.Setup(r => r.UpdateAsync(It.IsAny<Prayer>())).ReturnsAsync(true);
        tagsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Tag> { new Tag { Name = "Tag1", TagType = TagType.Prayer } });

        var updated = new NewPrayerDto { Title = "New", Description = "Desc", MarkdownContent = "Content", Image = "image.webp", TagIds = new List<int> { 1 } };
        var result = await service.UpdatePrayerAsync(1, updated, "user1");

        Assert.True(result);
        Assert.Equal("New", existing.Title);
        Assert.Equal("Desc", existing.Description);
        Assert.Single(existing.Tags);
        Assert.Equal("Tag1", existing.Tags.First().Name);
        Assert.Equal(TagType.Prayer, existing.Tags.First().TagType);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.Prayer, 1, "New", ActivityAction.Updated, "user1"), Times.Once);
    }
}
