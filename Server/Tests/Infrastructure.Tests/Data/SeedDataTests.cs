using Core.Enums;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Data;

public class SeedDataTests
{
    private static DataContext CreateContext([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"SeedDataTests_{testName}_{Guid.NewGuid()}")
            .Options;

        return new DataContext(options);
    }

    private static async Task AddTagAsync(DataContext context, string name, TagType tagType)
    {
        context.Tags.Add(new Tag { Name = name, TagType = tagType });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task SeedTags_ShouldAddTagsToDatabase()
    {
        using var context = CreateContext();

        Assert.Empty(context.Tags);

        await SeedData.SeedTags(context);

        Assert.NotEmpty(context.Tags);
        Assert.All(context.Tags, tag => Assert.False(string.IsNullOrWhiteSpace(tag.Name)));
    }

    [Fact]
    public async Task SeedTags_ShouldNotDuplicateExistingCaseInsensitiveTags()
    {
        using var context = CreateContext();

        context.Tags.Add(new Tag { Name = "FOUNDER", TagType = TagType.Saint });
        await context.SaveChangesAsync();

        await SeedData.SeedTags(context);

        var founderSaintCount = await context.Tags
            .CountAsync(t => t.TagType == TagType.Saint && t.Name.ToLower() == "founder");

        Assert.Equal(1, founderSaintCount);
    }

    [Fact]
    public async Task SeedSaints_ShouldAddSaintsWithExistingTags()
    {
        using var context = CreateContext();

        context.Tags.AddRange(
            new Tag { Name = "Founder", TagType = TagType.Saint },
            new Tag { Name = "Mystic", TagType = TagType.Saint }
        );
        await context.SaveChangesAsync();

        await SeedData.SeedSaints(context);

        Assert.NotEmpty(context.Saints);
        var saint = context.Saints.Include(s => s.Tags).First();
        Assert.NotNull(saint.Tags);
        Assert.All(saint.Tags, tag =>
            Assert.Contains(context.Tags, persistedTag => persistedTag.Id == tag.Id));
    }

    [Fact]
    public async Task SeedSaints_ShouldNotAddMore_WhenSaintsAlreadyExist()
    {
        using var context = CreateContext();

        context.Saints.Add(new Saint
        {
            Name = "Existing Saint",
            Slug = "existing-saint",
            Country = "Italy",
            Century = 13,
            Image = "existing.webp",
            Description = "Existing seeded saint",
            MarkdownPath = "existing-saint.md"
        });
        await context.SaveChangesAsync();

        await SeedData.SeedSaints(context);

        Assert.Equal(1, await context.Saints.CountAsync());
    }

    [Fact]
    public async Task SeedMiracles_ShouldAddMiraclesWithExistingTags()
    {
        using var context = CreateContext();

        await AddTagAsync(context, "Healing", TagType.Miracle);

        await SeedData.SeedMiracles(context);

        Assert.NotEmpty(context.Miracles);
        var miracle = context.Miracles.Include(m => m.Tags).First();
        Assert.NotNull(miracle.Tags);
        Assert.All(miracle.Tags, tag =>
            Assert.Contains(context.Tags, persistedTag => persistedTag.Id == tag.Id));
    }

    [Fact]
    public async Task SeedPrayers_ShouldAddPrayersWithExistingTags()
    {
        using var context = CreateContext();

        await AddTagAsync(context, "Marian", TagType.Prayer);

        await SeedData.SeedPrayers(context);

        Assert.NotEmpty(context.Prayers);
        var prayer = context.Prayers.Include(p => p.Tags).First();
        Assert.NotNull(prayer.Tags);
        Assert.All(prayer.Tags, tag =>
            Assert.Contains(context.Tags, persistedTag => persistedTag.Id == tag.Id));
    }
}
