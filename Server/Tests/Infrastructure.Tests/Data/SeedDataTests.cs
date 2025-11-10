using Microsoft.EntityFrameworkCore;
using Core.Models;
using Infrastructure.Data;

namespace Infrastructure.Tests.Data;

public class SeedDataTests
{
    private DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase($"SeedTest_{System.Guid.NewGuid()}")
            .Options;
        return new DataContext(options);
    }

    [Fact]
    public async Task SeedTags_ShouldAddTagsToDatabase()
    {
        using var context = CreateContext();

        // Garantir que não existe tag inicial
        Assert.Empty(context.Tags);

        await SeedData.SeedTags(context);

        Assert.NotEmpty(context.Tags);
        Assert.All(context.Tags, t => Assert.False(string.IsNullOrWhiteSpace(t.Name)));
    }

    [Fact]
    public async Task SeedSaints_ShouldAddSaintsWithExistingTags()
    {
        using var context = CreateContext();

        // Criar tags fictícias
        context.Tags.AddRange(new List<Tag>
        {
            new() { Name = "Founder", TagType = Core.Enums.TagType.Saint },
            new() { Name = "Mystic", TagType = Core.Enums.TagType.Saint }
        });
        await context.SaveChangesAsync();

        await SeedData.SeedSaints(context);

        Assert.NotEmpty(context.Saints);
        var saint = context.Saints.Include(s => s.Tags).First();
        Assert.NotNull(saint.Tags);
        Assert.All(saint.Tags, t => Assert.Contains(t, context.Tags));
    }

    [Fact]
    public async Task SeedMiracles_ShouldAddMiraclesWithExistingTags()
    {
        using var context = CreateContext();

        context.Tags.Add(new Tag { Name = "Healing", TagType = Core.Enums.TagType.Miracle });
        await context.SaveChangesAsync();

        await SeedData.SeedMiracles(context);

        Assert.NotEmpty(context.Miracles);
        var miracle = context.Miracles.Include(m => m.Tags).First();
        Assert.NotNull(miracle.Tags);
        Assert.All(miracle.Tags, t => Assert.Contains(t, context.Tags));
    }

    [Fact]
    public async Task SeedPrayers_ShouldAddPrayersWithExistingTags()
    {
        using var context = CreateContext();

        context.Tags.Add(new Tag { Name = "Marian", TagType = Core.Enums.TagType.Prayer });
        await context.SaveChangesAsync();

        await SeedData.SeedPrayers(context);

        Assert.NotEmpty(context.Prayers);
        var prayer = context.Prayers.Include(p => p.Tags).First();
        Assert.NotNull(prayer.Tags);
        Assert.All(prayer.Tags, t => Assert.Contains(t, context.Tags));
    }
}
