using Common;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class PrayersRepositoryTests
{
    private PrayersRepository CreateRepository(out DataContext context)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new DataContext(options);
        context.Database.EnsureCreated();

        var cache = new DummyCacheService();
        var repo = new PrayersRepository(context, cache);

        context.Prayers.AddRange(GetSeedData());
        context.SaveChanges();

        return repo;
    }

    private static List<Prayer> GetSeedData() => new()
    {
        new()
        {
            Title = "Hail Mary",
            Description = "A traditional Catholic prayer asking for the intercession of the Blessed Virgin Mary.",
            Slug = "hail-mary",
            Image = "img",
            MarkdownPath = "md",
            Tags = new() { new Tag { Name = "Marian", TagType = Core.Enums.TagType.Prayer } }
        },
        new()
        {
            Title = "Our Father",
            Description = "The prayer taught by Jesus Himself, central to Christian devotion.",
            Slug = "our-father",
            Image = "img",
            MarkdownPath = "md",
            Tags = new() { new Tag { Name = "Liturgical", TagType = Core.Enums.TagType.Prayer } }
        },
        new()
        {
            Title = "Prayer to St. Michael",
            Description = "A prayer for protection against evil, invoking the archangel Michael.",
            Slug = "prayer-to-st-michael",
            Image = "img",
            MarkdownPath = "md",
            Tags = new() { new Tag { Name = "Protection", TagType = Core.Enums.TagType.Prayer } }
        }
    };

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        var repo = CreateRepository(out _);
        var result = await repo.GetAllAsync(new PrayerFilters());
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCorrectPrayer()
    {
        var repo = CreateRepository(out _);
        var prayer = await repo.GetBySlugAsync("our-father");
        Assert.NotNull(prayer);
        Assert.Equal("Our Father", prayer!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectPrayer()
    {
        var repo = CreateRepository(out var context);
        var id = context.Prayers.First().Id;
        var prayer = await repo.GetByIdAsync(id);
        Assert.NotNull(prayer);
        Assert.Equal("Hail Mary", prayer!.Title);
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldReturnTrueIfExists()
    {
        var repo = CreateRepository(out _);
        var exists = await repo.SlugExistsAsync("hail-mary");
        Assert.True(exists);
    }

    [Fact]
    public async Task GetTagsAsync_ShouldReturnDistinctTags()
    {
        var repo = CreateRepository(out _);
        var tags = await repo.GetTagsAsync();
        Assert.Contains("Marian", tags);
        Assert.Contains("Liturgical", tags);
        Assert.Contains("Protection", tags);
        Assert.Equal(3, tags.Count);
    }

    [Fact]
    public async Task GetTotalPrayersAsync_ShouldReturnCorrectCount()
    {
        var repo = CreateRepository(out _);
        var total = await repo.GetTotalPrayersAsync();
        Assert.Equal(3, total);
    }
}
